using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using log4net.Config;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Service.Implementation;
using DominoTrainGame.Server.Service.Networking;

namespace DominoTrainGame.Server.Host;

internal static class Program
{
    private const int SuccessExitCode = 0;
    private const int FailureExitCode = 1;
    private const string LogConfigurationFileName = "log4net.config";

    private static readonly ITraceLog _logger;

    static Program()
    {
        _logger = TraceLogManager.GetLogger(typeof(Program));
    }

    private static int Main()
    {
        ConfigureLogging();
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

    private static void ConfigureLogging()
    {
        string logConfigurationPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            LogConfigurationFileName);
        XmlConfigurator.Configure(new FileInfo(logConfigurationPath));
    }

    private static void RunServer()
    {
        GameService gameService = GameServiceFactory.Create();
        ServiceHost serviceHost = new ServiceHost(gameService);
        GameChannelCloser channelCloser = new GameChannelCloser();

        try
        {
            serviceHost.Open();
            LogEndpoints(serviceHost);
            WaitForStopRequest();
        }
        finally
        {
            channelCloser.Close(serviceHost);
        }

        _logger.Info("Domino Train server stopped.");
    }

    private static void LogEndpoints(ServiceHost serviceHost)
    {
        foreach (ServiceEndpoint endpoint in serviceHost.Description.Endpoints)
        {
            _logger.InfoFormat("Endpoint listening at {0}.", endpoint.Address);
        }
    }

    private static void WaitForStopRequest()
    {
        using (ManualResetEventSlim stopSignal = new ManualResetEventSlim())
        {
            ConsoleCancelEventHandler cancelKeyPressHandler = (sender, e) => RequestStop(e, stopSignal);

            Console.CancelKeyPress += cancelKeyPressHandler;
            _logger.Info("Domino Train server is running. Press Ctrl+C to stop it.");
            stopSignal.Wait();
            Console.CancelKeyPress -= cancelKeyPressHandler;
        }
    }

    private static void RequestStop(ConsoleCancelEventArgs cancelEventArgs, ManualResetEventSlim stopSignal)
    {
        cancelEventArgs.Cancel = true;
        stopSignal.Set();
    }
}
