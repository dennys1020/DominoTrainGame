using System;
using System.IO;
using System.Windows;
using log4net.Config;
using DominoTrainGame.Client.Models;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.Networking;
using DominoTrainGame.Client.View.Navigation;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.View;

public sealed partial class App : Application
{
    private const string LogConfigurationFileName = "log4net.config";

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
    }

    private static IRemoteGameServices CreateGameServices(GameChannelCloser channelCloser)
    {
        IGameConnectionFactory connectionFactory = new GameConnectionFactory();
        ClientSession clientSession = new ClientSession();
        GameClientCallback lobbyCallback = new GameClientCallback(clientSession);
        LobbySessionContext sessionContext = new LobbySessionContext(clientSession, lobbyCallback);
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

    private void ShowLoginWindow(WindowFactory windowFactory)
    {
        Window loginWindow = windowFactory.Create(Screen.Login);

        loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        MainWindow = loginWindow;
        loginWindow.Show();
    }
}
