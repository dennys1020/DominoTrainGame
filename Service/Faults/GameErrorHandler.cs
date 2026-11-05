using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using log4net.Ext.Trace;

namespace DominoTrainGame.Server.Service.Faults;

public sealed class GameErrorHandler : IErrorHandler
{
    private static readonly ITraceLog _logger;

    static GameErrorHandler()
    {
        _logger = TraceLogManager.GetLogger(typeof(GameErrorHandler));
    }

    public bool HandleError(Exception error)
    {
        bool isDeclaredFault = error is FaultException;
        bool isCommunicationError = !isDeclaredFault && error is CommunicationException;

        if (isCommunicationError)
        {
            _logger.Warn("A communication error reached the service boundary.", error);
        }
        else if (!isDeclaredFault)
        {
            _logger.Error("An unexpected error reached the service boundary.", error);
        }

        return isDeclaredFault;
    }

    public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
    {
        bool isDeclaredFault = error is FaultException;

        if (!isDeclaredFault)
        {
            FaultException unexpectedFault = new FaultException();
            MessageFault messageFault = unexpectedFault.CreateMessageFault();
            fault = Message.CreateMessage(version, messageFault, unexpectedFault.Action);
        }
    }
}
