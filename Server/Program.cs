using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.Threading;
using log4net.Config;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Service;

namespace DominoTrainGame.Server.Host;

internal static class Program
{
    private const int SuccessExitCode = 0;
    private const int FailureExitCode = 1;
    private const string LogConfigurationFileName = "log4net.config";
    private static readonly ITraceLog _logger;
    private static readonly ManualResetEventSlim _stopSignal;

    static Program()
    {
        string logConfigurationPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            LogConfigurationFileName);
        XmlConfigurator.Configure(new FileInfo(logConfigurationPath));
        _logger = TraceLogManager.GetLogger(typeof(Program));
        _stopSignal = new ManualResetEventSlim();
    }

    private static int Main()
    {
        int exitCode = FailureExitCode;

        try
        {
            RunServer();
            exitCode = SuccessExitCode;
        }
        catch (ConfigurationErrorsException exception)
        {
            _logger.Fatal("The server configuration is missing or invalid.", exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Fatal("The server endpoints could not be opened.", exception);
        }
        catch (TimeoutException exception)
        {
            _logger.Fatal("The server endpoints did not open before the configured timeout.", exception);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Fatal("The service host configuration is invalid.", exception);
        }

        return exitCode;
    }

    private static void RunServer()
    {
        using (GameServiceRuntime serviceRuntime = new GameServiceRuntime())
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            serviceRuntime.Start();
            _logger.Info("Domino Train server is running. Press Ctrl+C to stop it.");
            _stopSignal.Wait();
        }

        _logger.Info("Domino Train server stopped.");
    }

    private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        _stopSignal.Set();
    }
}
