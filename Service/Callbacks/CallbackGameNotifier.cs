using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Service.Callbacks;

public sealed class CallbackGameNotifier : IGameNotifier
{
    private readonly PlayerSessionRegistry<IGameClientCallback> _lobbySessions;

    public CallbackGameNotifier(PlayerSessionRegistry<IGameClientCallback> lobbySessions)
    {
        _lobbySessions = lobbySessions ?? throw new ArgumentNullException(nameof(lobbySessions));
    }

    public void NotifyPlayerJoined(PlayerJoinedNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        PlayerJoinedMessage message = new PlayerJoinedMessage
        {
            Player = new LobbyPlayer
            {
                PlayerId = notice.Member.PlayerId,
                Username = notice.Member.Username
            },
            Version = notice.Version
        };

        Broadcast(notice.RecipientPlayerIds, callback => callback.ReceivePlayerJoined(message));
    }

    public void NotifyPlayerLeft(PlayerLeftNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        PlayerLeftMessage message = new PlayerLeftMessage
        {
            PlayerId = notice.PlayerId,
            Version = notice.Version,
            HostPlayerId = notice.HostPlayerId
        };

        Broadcast(notice.RecipientPlayerIds, callback => callback.ReceivePlayerLeft(message));
    }

    public void NotifyPlayerReadyChanged(PlayerReadyChangedNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        PlayerReadyChangedMessage message = new PlayerReadyChangedMessage
        {
            Version = notice.Version,
            PlayerId = notice.Player.PlayerId,
            IsReady = notice.Player.IsReady
        };

        Broadcast(notice.RecipientPlayerIds, callback => callback.ReceivePlayerReadyChanged(message));
    }

    public void NotifyLobbyMessage(LobbyMessageNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        LobbyChatMessage message = new LobbyChatMessage
        {
            PlayerId = notice.Sender.PlayerId,
            Username = notice.Sender.Username,
            Text = notice.Text
        };

        Broadcast(notice.RecipientPlayerIds, callback => callback.ReceiveLobbyMessage(message));
    }

    public void NotifyMatchStarting(MatchStartingNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        MatchStartingMessage message = new MatchStartingMessage
        {
            Code = notice.Code
        };

        Broadcast(notice.RecipientPlayerIds, callback => callback.ReceiveMatchStarting(message));
    }

    public bool TryNotifyLobbyInvitation(LobbyInvitationNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        LobbyInvitationMessage message = new LobbyInvitationMessage
        {
            InviterUsername = notice.Inviter.Username,
            Code = notice.Code
        };
        bool isConnected = _lobbySessions.TrySend(
            notice.RecipientPlayerId,
            callback => callback.ReceiveLobbyInvitation(message));

        return isConnected;
    }

    private void Broadcast(IReadOnlyList<int> recipientPlayerIds, Action<IGameClientCallback> send)
    {
        foreach (int recipientPlayerId in recipientPlayerIds)
        {
            _lobbySessions.TrySend(recipientPlayerId, send);
        }
    }
}
