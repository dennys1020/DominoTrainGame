using System;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Implementation;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Service.Callbacks;

public sealed class CallbackSocialNotifier : ISocialNotifier
{
    private readonly PlayerSessionRegistry<ISocialClientCallback> _socialSessions;

    public CallbackSocialNotifier(PlayerSessionRegistry<ISocialClientCallback> socialSessions)
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
            _socialSessions.TrySend(recipientPlayerId, callback => callback.ReceiveFriendStatusChanged(message));
        }
    }

    public void NotifyFriendsChanged(int playerId)
    {
        _socialSessions.TrySend(playerId, callback => callback.ReceiveFriendsChanged());
    }
}
