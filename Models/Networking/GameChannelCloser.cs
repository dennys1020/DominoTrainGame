using System;
using System.ServiceModel;
using log4net.Ext.Trace;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class GameChannelCloser
{
    private static readonly ITraceLog _logger;

    static GameChannelCloser()
    {
        _logger = TraceLogManager.GetLogger(typeof(GameChannelCloser));
    }

    public void Close(ICommunicationObject channel)
    {
        if (channel == null)
        {
            throw new ArgumentNullException(nameof(channel));
        }

        try
        {
            CloseOrAbort(channel);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn(
                "The communication object did not close before the configured timeout; it was aborted.",
                exception);
            channel.Abort();
        }
        catch (CommunicationException exception)
        {
            _logger.Warn(
                "The communication object failed to close because of a communication error; it was aborted.",
                exception);
            channel.Abort();
        }
    }

    private static void CloseOrAbort(ICommunicationObject channel)
    {
        if (channel.State == CommunicationState.Faulted)
        {
            channel.Abort();
        }
        else
        {
            channel.Close();
        }
    }
}
