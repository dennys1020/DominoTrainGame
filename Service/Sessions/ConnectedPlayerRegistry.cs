using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Service.Contracts;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class ConnectedPlayerRegistry
{
    private static readonly ITraceLog _logger;

    private readonly ConcurrentDictionary<int, ConnectedPlayer> _playersById =
        new ConcurrentDictionary<int, ConnectedPlayer>();

    static ConnectedPlayerRegistry()
    {
        _logger = TraceLogManager.GetLogger(typeof(ConnectedPlayerRegistry));
    }

    public bool TryAdd(ConnectedPlayer connectedPlayer)
    {
        if (connectedPlayer == null)
        {
            throw new ArgumentNullException(nameof(connectedPlayer));
        }

        bool wasAdded = _playersById.TryAdd(
            connectedPlayer.Player.PlayerId,
            connectedPlayer);

        return wasAdded;
    }

    public bool TryGetPlayer(
        string sessionId,
        out AuthenticatedPlayer player)
    {
        if (sessionId == null)
        {
            throw new ArgumentNullException(nameof(sessionId));
        }

        bool wasFound = false;
        player = AuthenticatedPlayer.Empty;

        foreach (ConnectedPlayer connectedPlayer in _playersById.Values)
        {
            if (IsSameSession(connectedPlayer, sessionId))
            {
                player = connectedPlayer.Player;
                wasFound = true;
            }
        }

        return wasFound;
    }

    public bool TryGetCallback(
        int playerId,
        out IGameClientCallback callback)
    {
        bool wasFound = _playersById.TryGetValue(
            playerId,
            out ConnectedPlayer connectedPlayer);

        callback = wasFound
            ? connectedPlayer.Callback
            : null;

        return wasFound;
    }

    public bool IsConnected(int playerId)
    {
        bool isConnected = _playersById.ContainsKey(playerId);
        return isConnected;
    }

    public bool TryDisconnect(int playerId)
    {
        bool wasRemoved = _playersById.TryRemove(playerId, out ConnectedPlayer removedPlayer);

        if (wasRemoved)
        {
            AbortChannel(removedPlayer.Callback);
        }

        return wasRemoved;
    }

    public bool Remove(
        string sessionId,
        out AuthenticatedPlayer player)
    {
        if (sessionId == null)
        {
            throw new ArgumentNullException(nameof(sessionId));
        }

        bool wasRemoved = false;
        player = AuthenticatedPlayer.Empty;

        foreach (KeyValuePair<int, ConnectedPlayer> entry in _playersById)
        {
            bool isPendingMatch = !wasRemoved && IsSameSession(entry.Value, sessionId);

            if (isPendingMatch)
            {
                wasRemoved = TryRemoveEntry(entry);
                player = wasRemoved ? entry.Value.Player : player;
            }
        }

        return wasRemoved;
    }

    private static bool IsSameSession(ConnectedPlayer connectedPlayer, string sessionId)
    {
        bool isSameSession = string.Equals(connectedPlayer.SessionId, sessionId, StringComparison.Ordinal);
        return isSameSession;
    }

    private static void AbortChannel(IGameClientCallback callback)
    {
        if (callback is ICommunicationObject channel)
        {
            try
            {
                channel.Abort();
            }
            catch (CommunicationException exception)
            {
                _logger.Warn("A stale channel could not be aborted cleanly.", exception);
            }
        }
    }

    private bool TryRemoveEntry(KeyValuePair<int, ConnectedPlayer> entry)
    {
        ICollection<KeyValuePair<int, ConnectedPlayer>> entries = _playersById;
        bool wasRemoved = entries.Remove(entry);
        return wasRemoved;
    }
}
