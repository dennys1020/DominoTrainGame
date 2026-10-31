using System;
using System.ServiceModel;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;
using DominoTrainGame.Server.Service.Networking;
using log4net.Ext.Trace;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed class CallbackGameNotifier : IGameNotifier
{
    private static readonly ITraceLog _logger;

    private readonly ConnectedPlayerRegistry _connectedPlayers;

    static CallbackGameNotifier()
    {
        _logger = TraceLogManager.GetLogger(typeof(CallbackGameNotifier));
    }

    public CallbackGameNotifier(ConnectedPlayerRegistry connectedPlayers)
    {
        _connectedPlayers = connectedPlayers
            ?? throw new ArgumentNullException(nameof(connectedPlayers));
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
                IsHost = false
            },
            Version = notice.Version
        };

        foreach (int recipientPlayerId in notice.RecipientPlayerIds)
        {
            bool hasCallback = _connectedPlayers.TryGetCallback(
                recipientPlayerId,
                out IGameClientCallback callback);

            if (hasCallback)
            {
                TrySendPlayerJoined(callback, message);
            }
        }
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

        foreach (int recipientPlayerId in notice.RecipientPlayerIds)
        {
            bool hasCallback = _connectedPlayers.TryGetCallback(
                recipientPlayerId,
                out IGameClientCallback callback);

            if (hasCallback)
            {
                TrySendPlayerLeft(callback, message);
            }
        }
    }

    private static void TrySendPlayerJoined(
        IGameClientCallback callback,
        PlayerJoinedMessage message)
    {
        try
        {
            callback.ReceivePlayerJoined(message);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn(
                "A player did not receive the join notice before the timeout.",
                exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn(
                "A player did not receive the join notice because of a communication error.",
                exception);
        }
    }

    private static void TrySendPlayerLeft(
        IGameClientCallback callback,
        PlayerLeftMessage message)
    {
        try
        {
            callback.ReceivePlayerLeft(message);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn(
                "A player did not receive the leave notice before the timeout.",
                exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn(
                "A player did not receive the leave notice because of a communication error.",
                exception);
        }
    }
}
