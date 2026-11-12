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

    public FaultException<GameServiceFault> TranslateInvalidInput(
        ArgumentException exception,
        GameServiceFaultCode invalidInputFaultCode)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        _logger.Warn(CreateLogMessage(InvalidInputMessageFormat), exception);
        FaultException<GameServiceFault> fault = GameFaultFactory.Create(invalidInputFaultCode);

        return fault;
    }

    public FaultException<GameServiceFault> TranslateDatabaseUnavailable(DatabaseUnavailableException exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        _logger.Error(CreateLogMessage(DatabaseUnavailableMessageFormat), exception);
        FaultException<GameServiceFault> fault = GameFaultFactory.Create(GameServiceFaultCode.DatabaseUnavailable);

        return fault;
    }

    private static string CreateLogMessage(string messageFormat)
    {
        string operationName = OperationContext.Current?.IncomingMessageHeaders.Action ?? string.Empty;
        string message = string.Format(CultureInfo.InvariantCulture, messageFormat, operationName);
        return message;
    }
}
