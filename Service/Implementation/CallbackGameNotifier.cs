using System;
using System.Collections.Generic;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;
using DominoTrainGame.Server.Service.Networking;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed class CallbackGameNotifier : IGameNotifier
{
    private const bool JoiningPlayerHostState = false;

    private static readonly ITraceLog _logger;

    private readonly ConnectedPlayerRegistry _connectedPlayers;

    static CallbackGameNotifier()
    {
        _logger = TraceLogManager.GetLogger(typeof(CallbackGameNotifier));
    }

    public CallbackGameNotifier(ConnectedPlayerRegistry connectedPlayers)
    {
        _connectedPlayers = connectedPlayers ?? throw new ArgumentNullException(nameof(connectedPlayers));
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
                Username = notice.Member.Username,
                IsHost = JoiningPlayerHostState
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

    private void Broadcast(IReadOnlyList<int> recipientPlayerIds, Action<IGameClientCallback> send)
    {
        foreach (int recipientPlayerId in recipientPlayerIds)
        {
            bool hasCallback = _connectedPlayers.TryGetCallback(recipientPlayerId, out IGameClientCallback callback);

            if (hasCallback)
            {
                TrySend(callback, send);
            }
        }
    }

    private static void TrySend(IGameClientCallback callback, Action<IGameClientCallback> send)
    {
        try
        {
            send(callback);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn("A player did not receive a lobby notice before the timeout.", exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn("A player did not receive a lobby notice because of a communication error.", exception);
        }
        catch (ObjectDisposedException exception)
        {
            _logger.Warn("A player did not receive a lobby notice because the channel was already closed.", exception);
        }
    }
}
