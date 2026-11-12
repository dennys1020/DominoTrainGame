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
        IGameConnectionFactory connectionFactory = new GameConnectionFactory();
        RemoteServices remoteServices = CreateRemoteServices(connectionFactory, channelCloser);
        LocalizationViewModel localization = LocalizationViewModel.Instance;
        WindowFactory windowFactory = new WindowFactory(remoteServices, localization);

        ShowLoginWindow(windowFactory);
    }

    private static RemoteServices CreateRemoteServices(
        IGameConnectionFactory connectionFactory,
        GameChannelCloser channelCloser)
    {
        ClientSession clientSession = new ClientSession();
        GameClientCallback lobbyCallback = new GameClientCallback(clientSession);
        LobbyChannel lobbyChannel = new LobbyChannel(connectionFactory, channelCloser, clientSession, lobbyCallback);
        AccountRequests accountRequests = new AccountRequests(
            connectionFactory,
            channelCloser,
            clientSession,
            lobbyChannel);
        IRemoteGameServices gameServices = new RemoteGameServices(
            accountRequests,
            lobbyChannel,
            clientSession,
            lobbyCallback);
        SocialClientCallback socialCallback = new SocialClientCallback();
        IRemoteSocialServices socialServices = new RemoteSocialServices(
            connectionFactory,
            channelCloser,
            gameServices,
            socialCallback);
        MatchRequests matchRequests = new MatchRequests(connectionFactory, channelCloser, clientSession);
        IRemoteMatchServices matchServices = new RemoteMatchServices(matchRequests, clientSession, lobbyCallback);
        RemoteServices remoteServices = new RemoteServices(gameServices, socialServices, matchServices);
        return remoteServices;
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
