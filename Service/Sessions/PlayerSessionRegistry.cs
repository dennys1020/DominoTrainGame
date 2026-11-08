using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class PlayerSessionRegistry<TCallback> : IPresenceTracker
    where TCallback : class
{
    private const int FirstMatchIndex = 0;

    private static readonly ITraceLog _logger;

    private readonly ConcurrentDictionary<int, PlayerSession<TCallback>> _sessionsByPlayerId;

    static PlayerSessionRegistry()
    {
        _logger = TraceLogManager.GetLogger(typeof(PlayerSessionRegistry<TCallback>));
    }

    public PlayerSessionRegistry()
    {
        _sessionsByPlayerId = new ConcurrentDictionary<int, PlayerSession<TCallback>>();
    }

    public bool TryAdd(PlayerSession<TCallback> session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        bool wasAdded = _sessionsByPlayerId.TryAdd(session.Player.PlayerId, session);
        return wasAdded;
    }

    public bool IsOnline(int playerId)
    {
        bool isOnline = _sessionsByPlayerId.ContainsKey(playerId);
        return isOnline;
    }

    public bool TryGetPlayer(string sessionId, out PlayerIdentity player)
    {
        if (sessionId == null)
        {
            throw new ArgumentNullException(nameof(sessionId));
        }

        List<PlayerIdentity> matchingPlayers = _sessionsByPlayerId.Values
            .Where(session => string.Equals(session.SessionId, sessionId, StringComparison.Ordinal))
            .Select(session => session.Player)
            .ToList();
        bool wasFound = matchingPlayers.Any();

        player = wasFound ? matchingPlayers[FirstMatchIndex] : PlayerIdentity.Empty;
        return wasFound;
    }

    public bool TrySend(int playerId, Action<TCallback> send)
    {
        if (send == null)
        {
            throw new ArgumentNullException(nameof(send));
        }

        bool isConnected = _sessionsByPlayerId.TryGetValue(playerId, out PlayerSession<TCallback> session);

        if (isConnected)
        {
            SendSafely(session.Callback, send);
        }

        return isConnected;
    }

    public bool TryDisconnect(int playerId)
    {
        bool wasRemoved = _sessionsByPlayerId.TryRemove(playerId, out PlayerSession<TCallback> removedSession);

        if (wasRemoved)
        {
            AbortChannel(removedSession.Callback);
        }

        return wasRemoved;
    }

    public bool Remove(PlayerSession<TCallback> session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        ICollection<KeyValuePair<int, PlayerSession<TCallback>>> entries = _sessionsByPlayerId;
        KeyValuePair<int, PlayerSession<TCallback>> entry = new KeyValuePair<int, PlayerSession<TCallback>>(
            session.Player.PlayerId,
            session);
        bool wasRemoved = entries.Remove(entry);

        return wasRemoved;
    }

    private static void SendSafely(TCallback callback, Action<TCallback> send)
    {
        try
        {
            send(callback);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn("A player did not receive a notice before the timeout.", exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn("A player did not receive a notice because of a communication error.", exception);
        }
        catch (ObjectDisposedException exception)
        {
            _logger.Warn("A player did not receive a notice because the channel was already closed.", exception);
        }
    }

    private static void AbortChannel(TCallback callback)
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
}
