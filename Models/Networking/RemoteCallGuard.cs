using System;
using System.Globalization;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal sealed class RemoteCallGuard
{
    private const string RejectedMessageFormat = "The server rejected a {0} request. FaultCode: {1}.";
    private const string UndeclaredFaultMessageFormat =
        "The server reported an undeclared fault while processing a {0} request.";
    private const string TimeoutMessageFormat = "A {0} request did not finish before the configured timeout.";
    private const string CommunicationMessageFormat = "A {0} request could not reach the server.";
    private const string DisposedChannelMessageFormat = "A {0} request used a channel that was already closed.";
    private const string InvalidEndpointMessageFormat =
        "A {0} request could not be sent because the endpoint is invalid or the channel rejected the call.";

    private static readonly ITraceLog _logger;

    static RemoteCallGuard()
    {
        _logger = TraceLogManager.GetLogger(typeof(RemoteCallGuard));
    }

    public static TStatus KeepStatus<TStatus>(TStatus status)
    {
        return status;
    }

    public static async Task<TStatus> CompleteCommandAsync<TStatus>(Task command, TStatus successStatus)
    {
        await command.ConfigureAwait(false);
        return successStatus;
    }

    public Task<TStatus> ExecuteAsync<TStatus>(Func<Task<TStatus>> call, ServiceFailureMapping<TStatus> failures)
    {
        Task<TStatus> execution = ExecuteAsync(call, failures, KeepStatus<TStatus>);
        return execution;
    }

    public Task<TResult> ExecuteAsync<TStatus, TResult>(
        Func<Task<TResult>> call,
        ServiceFailureMapping<TStatus> failures,
        Func<TStatus, TResult> createFailure)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        if (failures == null)
        {
            throw new ArgumentNullException(nameof(failures));
        }

        if (createFailure == null)
        {
            throw new ArgumentNullException(nameof(createFailure));
        }

        Task<TResult> execution = ExecuteCoreAsync(call, failures, createFailure);
        return execution;
    }

    private static async Task<TResult> ExecuteCoreAsync<TStatus, TResult>(
        Func<Task<TResult>> call,
        ServiceFailureMapping<TStatus> failures,
        Func<TStatus, TResult> createFailure)
    {
        TResult result;

        try
        {
            result = await call().ConfigureAwait(false);
        }
        catch (FaultException<GameServiceFault> exception)
        {
            _logger.WarnFormat(RejectedMessageFormat, failures.OperationName, exception.Detail.Code);
            result = createFailure(failures.MapFault(exception.Detail.Code));
        }
        catch (FaultException exception)
        {
            _logger.Error(CreateLogMessage(UndeclaredFaultMessageFormat, failures.OperationName), exception);
            result = createFailure(failures.ServerError);
        }
        catch (TimeoutException exception)
        {
            _logger.Error(CreateLogMessage(TimeoutMessageFormat, failures.OperationName), exception);
            result = createFailure(failures.ServerUnavailable);
        }
        catch (CommunicationException exception)
        {
            _logger.Error(CreateLogMessage(CommunicationMessageFormat, failures.OperationName), exception);
            result = createFailure(failures.ServerUnavailable);
        }
        catch (ObjectDisposedException exception)
        {
            _logger.Error(CreateLogMessage(DisposedChannelMessageFormat, failures.OperationName), exception);
            result = createFailure(failures.ServerUnavailable);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error(CreateLogMessage(InvalidEndpointMessageFormat, failures.OperationName), exception);
            result = createFailure(failures.ServerError);
        }

        return result;
    }

    private static string CreateLogMessage(string messageFormat, string operationName)
    {
        string message = string.Format(CultureInfo.InvariantCulture, messageFormat, operationName);
        return message;
    }
}
