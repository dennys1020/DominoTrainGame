using System;
using System.Globalization;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Errors;

namespace DominoTrainGame.Server.Service.Faults;

public sealed class ServiceFaultTranslator
{
    private const string InvalidInputMessageFormat =
        "A request was rejected because its input is invalid. Operation: {0}.";
    private const string DatabaseUnavailableMessageFormat =
        "A request failed because the database is unavailable. Operation: {0}.";

    private static readonly ITraceLog _logger;

    static ServiceFaultTranslator()
    {
        _logger = TraceLogManager.GetLogger(typeof(ServiceFaultTranslator));
    }

    public TResult Execute<TResult>(Func<TResult> request, GameServiceFaultCode invalidInputFaultCode)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        TResult result;

        try
        {
            result = request();
        }
        catch (ArgumentException exception)
        {
            _logger.Warn(CreateLogMessage(InvalidInputMessageFormat), exception);
            throw GameFaultFactory.Create(invalidInputFaultCode);
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error(CreateLogMessage(DatabaseUnavailableMessageFormat), exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.DatabaseUnavailable);
        }

        return result;
    }

    private static string CreateLogMessage(string messageFormat)
    {
        string operationName = OperationContext.Current?.IncomingMessageHeaders.Action ?? string.Empty;
        string message = string.Format(CultureInfo.InvariantCulture, messageFormat, operationName);
        return message;
    }
}
