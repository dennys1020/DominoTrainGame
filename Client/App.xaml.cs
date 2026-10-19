using System;
using System.Configuration;
using System.Windows;
using log4net.Config;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Navigation;
using DominoTrainGame.ViewModels.Navigation;

namespace DominoTrainGame;

public partial class App : Application
{
    private static readonly ITraceLog _logger;
    private readonly IRemoteGameServices _gameServices;

    static App()
    {
        XmlConfigurator.Configure();
        _logger = TraceLogManager.GetLogger(typeof(App));
    }

    public App()
    {
        _gameServices = new RemoteGameServices();
    }

    private async void OnApplicationStartup(object sender, StartupEventArgs e)
    {
        ShowLoginWindow();

        try
        {
            ServerConnectionStatus connectionStatus = await _gameServices.CheckConnectionAsync();
            _logger.InfoFormat("Server connection check finished. Status: {0}.", connectionStatus);
        }
        catch (ConfigurationErrorsException exception)
        {
            _logger.Fatal("The client configuration is missing or invalid.", exception);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Fatal("The service endpoint was not found in App.config.", exception);
        }
    }

    private void ShowLoginWindow()
    {
        Window loginWindow = WindowFactory.Create(Screen.Login);

        loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        MainWindow = loginWindow;
        loginWindow.Show();
    }
}
