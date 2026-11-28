using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.Networking;
using DominoTrainGame.Client.View.Navigation;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;
using DominoTrainGame.Shared.Logging;
using DominoTrainGame.Shared.Networking;

namespace DominoTrainGame.Client.View;

public sealed partial class App : Application
{
    private const string UnexpectedErrorMessageKey = "UnexpectedErrorMessage";

    private static readonly ITraceLog _logger;

    private WindowFactory _windowFactory;

    static App()
    {
        _logger = TraceLogManager.GetLogger(typeof(App));
    }

    protected override void OnStartup(StartupEventArgs eventArgs)
    {
        LogConfigurator.Configure();
        RegisterUnhandledErrorLogging();
        base.OnStartup(eventArgs);

        GameChannelCloser channelCloser = new GameChannelCloser();
        IGameConnectionFactory connectionFactory = new GameConnectionFactory();
        RemoteServices remoteServices = CreateRemoteServices(connectionFactory, channelCloser);
        LocalizationViewModel localization = LocalizationViewModel.Instance;
        _windowFactory = new WindowFactory(remoteServices, localization);

        ShowLoginWindow(_windowFactory);
    }

    private static RemoteServices CreateRemoteServices(
        IGameConnectionFactory connectionFactory,
        GameChannelCloser channelCloser)
    {
        ClientSession clientSession = new ClientSession();
        GameClientCallback lobbyCallback = new GameClientCallback(clientSession);
        LobbyChannel lobbyChannel = new LobbyChannel(connectionFactory, channelCloser, clientSession, lobbyCallback);
        IRemoteAuthenticationServices authenticationServices = new RemoteAuthenticationServices(
            connectionFactory,
            channelCloser,
            lobbyChannel,
            clientSession);
        IRemoteGameServices gameServices = new RemoteGameServices(lobbyChannel, clientSession, lobbyCallback);
        SocialClientCallback socialCallback = new SocialClientCallback();
        IRemoteSocialServices socialServices = new RemoteSocialServices(
            connectionFactory,
            channelCloser,
            gameServices,
            socialCallback);
        IRemoteMatchServices matchServices = new RemoteMatchServices(
            connectionFactory,
            channelCloser,
            clientSession,
            lobbyCallback);
        IRemoteProfileServices profileServices = new RemoteProfileServices(
            connectionFactory,
            channelCloser,
            clientSession);
        RemoteServices remoteServices = new RemoteServices(
            authenticationServices,
            gameServices,
            socialServices,
            matchServices,
            profileServices);
        return remoteServices;
    }

    private static void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs eventArgs)
    {
        _logger.Error("A background task failed and nobody observed its error.", eventArgs.Exception);
    }

    private static void HandleDomainUnhandledException(object sender, UnhandledExceptionEventArgs eventArgs)
    {
        _logger.FatalFormat("An unhandled error is closing the game. Error: {0}", eventArgs.ExceptionObject);
    }

    private void RegisterUnhandledErrorLogging()
    {
        DispatcherUnhandledException += HandleDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += HandleDomainUnhandledException;
    }

    private void HandleDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
    {
        Window currentWindow = MainWindow;
        bool canShowMessage = _windowFactory != null && currentWindow != null;

        _logger.Error("An unexpected error reached the user interface.", eventArgs.Exception);
        eventArgs.Handled = canShowMessage;

        if (canShowMessage)
        {
            WindowService windowService = new WindowService(
                currentWindow,
                _windowFactory,
                LocalizationViewModel.Instance);
            windowService.ShowMessageError(UnexpectedErrorMessageKey);
        }
    }

    private void ShowLoginWindow(WindowFactory windowFactory)
    {
        Window loginWindow = windowFactory.Create(Screen.Login);

        loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        MainWindow = loginWindow;
        loginWindow.Show();
    }
}
