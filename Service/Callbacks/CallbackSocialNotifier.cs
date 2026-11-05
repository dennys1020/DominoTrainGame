using System;
using System.Collections.Generic;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Implementation;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Service.Callbacks;

public sealed class CallbackSocialNotifier : ISocialNotifier
{
    private static readonly ITraceLog _logger;

    private readonly SocialSessionRegistry _socialSessions;

    static CallbackSocialNotifier()
    {
        _logger = TraceLogManager.GetLogger(typeof(CallbackSocialNotifier));
    }

    public CallbackSocialNotifier(SocialSessionRegistry socialSessions)
    {
        _socialSessions = socialSessions ?? throw new ArgumentNullException(nameof(socialSessions));
    }

    public void NotifyFriendStatusChanged(FriendStatusNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        FriendStatusMessage message = new FriendStatusMessage
        {
            PlayerId = notice.PlayerId,
            Status = PlayerStatusCodes.FromStatus(notice.Status)
        };

        foreach (int recipientPlayerId in notice.RecipientPlayerIds)
        {
            Send(recipientPlayerId, callback => callback.ReceiveFriendStatusChanged(message));
        }
    }

    public void NotifyFriendsChanged(int playerId)
    {
        Send(playerId, callback => callback.ReceiveFriendsChanged());
    }

    private static void TrySend(ISocialClientCallback callback, Action<ISocialClientCallback> send)
    {
        try
        {
            send(callback);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn("A player did not receive a social notice before the timeout.", exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn("A player did not receive a social notice because of a communication error.", exception);
        }
        catch (ObjectDisposedException exception)
        {
            _logger.Warn("A player did not receive a social notice because the channel was already closed.", exception);
        }
    }

    private void Send(int recipientPlayerId, Action<ISocialClientCallback> send)
    {
        IReadOnlyList<ISocialClientCallback> callbacks = _socialSessions.FindCallbacks(recipientPlayerId);

        foreach (ISocialClientCallback callback in callbacks)
        {
            TrySend(callback, send);
        }
    }
}
