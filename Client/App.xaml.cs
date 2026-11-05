using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using log4net.Config;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.Networking;
using DominoTrainGame.Client.View.Navigation;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.View;

public sealed partial class App : Application
{
    private const string LogConfigurationFileName = "log4net.config";

    private static readonly ITraceLog _logger;

    static App()
    {
        _logger = TraceLogManager.GetLogger(typeof(App));
    }

    protected override void OnStartup(StartupEventArgs eventArgs)
    {
        ConfigureLogging();
        base.OnStartup(eventArgs);

        GameChannelCloser channelCloser = new GameChannelCloser();
        IRemoteGameServices gameServices = CreateGameServices(channelCloser);
        ISocialConnectionFactory socialConnectionFactory = new SocialConnectionFactory();
        IRemoteSocialServices socialServices = new RemoteSocialServices(
            socialConnectionFactory,
            channelCloser,
            gameServices);
        LocalizationViewModel localization = LocalizationViewModel.Instance;
        WindowFactory windowFactory = new WindowFactory(gameServices, socialServices, localization);

        ShowLoginWindow(windowFactory);
        StartConnectionCheck(gameServices);
    }

    private static IRemoteGameServices CreateGameServices(GameChannelCloser channelCloser)
    {
        IGameConnectionFactory connectionFactory = new GameConnectionFactory();
        ClientSession clientSession = new ClientSession();
        LobbyEventRelay lobbyEvents = new LobbyEventRelay(clientSession);
        LobbySessionContext sessionContext = new LobbySessionContext(clientSession, lobbyEvents);
        AccountRequests accountRequests = new AccountRequests(connectionFactory, channelCloser);
        LobbyChannel lobbyChannel = new LobbyChannel(connectionFactory, channelCloser, sessionContext);
        IRemoteGameServices gameServices = new RemoteGameServices(accountRequests, lobbyChannel, sessionContext);
        return gameServices;
    }

    private static void ConfigureLogging()
    {
        string logConfigurationPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            LogConfigurationFileName);
        XmlConfigurator.Configure(new FileInfo(logConfigurationPath));
    }

    private static void StartConnectionCheck(IRemoteGameServices gameServices)
    {
        AsyncRelayCommand checkConnectionCommand = new AsyncRelayCommand(
            () => CheckConnectionAsync(gameServices));
        checkConnectionCommand.Execute(null);
    }

    private static async Task CheckConnectionAsync(IRemoteGameServices gameServices)
    {
        ConnectionCheckResult connectionResult = await gameServices.CheckConnectionAsync();
        _logger.InfoFormat("Server connection check finished. Result: {0}.", connectionResult);
    }

    private void ShowLoginWindow(WindowFactory windowFactory)
    {
        Window loginWindow = windowFactory.Create(Screen.Login);

        loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        MainWindow = loginWindow;
        loginWindow.Show();
    }
}
