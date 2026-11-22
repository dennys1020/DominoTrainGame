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
    private const string EmailUnavailableMessageFormat =
        "A request failed because the verification email could not be sent. Operation: {0}.";
    private const string DataIntegrityMessageFormat =
        "A request was rejected because its data violates the database rules. Operation: {0}.";

    private static readonly ITraceLog _logger;

    static ServiceFaultTranslator()
    {
        _logger = TraceLogManager.GetLogger(typeof(ServiceFaultTranslator));
    }

    public TResult Execute<TResult>(Func<TResult> operation)
    {
        TResult result = Execute(operation, GameServiceFaultCode.InvalidInput);
        return result;
    }

    public TResult Execute<TResult>(Func<TResult> operation, GameServiceFaultCode invalidInputFaultCode)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        TResult result;

        try
        {
            result = operation();
        }
        catch (ArgumentException exception)
        {
            throw TranslateInvalidInput(exception, invalidInputFaultCode);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw TranslateDatabaseUnavailable(exception);
        }
        catch (EmailUnavailableException exception)
        {
            throw TranslateEmailUnavailable(exception);
        }
        catch (DataIntegrityException exception)
        {
            throw CreateLoggedFault(DataIntegrityMessageFormat, exception, invalidInputFaultCode);
        }

        return result;
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

        FaultException<GameServiceFault> fault = CreateLoggedFault(
            DatabaseUnavailableMessageFormat,
            exception,
            GameServiceFaultCode.DatabaseUnavailable);

        return fault;
    }

    public FaultException<GameServiceFault> TranslateEmailUnavailable(EmailUnavailableException exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        FaultException<GameServiceFault> fault = CreateLoggedFault(
            EmailUnavailableMessageFormat,
            exception,
            GameServiceFaultCode.EmailUnavailable);

        return fault;
    }

    private static FaultException<GameServiceFault> CreateLoggedFault(
        string messageFormat,
        Exception exception,
        GameServiceFaultCode faultCode)
    {
        _logger.Error(CreateLogMessage(messageFormat), exception);
        FaultException<GameServiceFault> fault = GameFaultFactory.Create(faultCode);

        return fault;
    }

    private static string CreateLogMessage(string messageFormat)
    {
        string operationName = OperationContext.Current?.IncomingMessageHeaders.Action ?? string.Empty;
        string message = string.Format(CultureInfo.InvariantCulture, messageFormat, operationName);
        return message;
    }
}
