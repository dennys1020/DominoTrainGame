using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class PlayerSessionRegistry<TCallback> : IPresenceTracker
    where TCallback : class
{
    private static readonly ITraceLog _logger;

    private readonly ConcurrentDictionary<int, PlayerSession<TCallback>> _sessionsByPlayerId;
    private readonly ConcurrentDictionary<string, int> _playerIdsBySessionId;

    static PlayerSessionRegistry()
    {
        _logger = TraceLogManager.GetLogger(typeof(PlayerSessionRegistry<TCallback>));
    }

    public PlayerSessionRegistry()
    {
        _sessionsByPlayerId = new ConcurrentDictionary<int, PlayerSession<TCallback>>();
        _playerIdsBySessionId = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
    }

    public bool TryAdd(PlayerSession<TCallback> session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        bool wasAdded = _sessionsByPlayerId.TryAdd(session.Player.PlayerId, session);

        if (wasAdded)
        {
            _playerIdsBySessionId[session.SessionId] = session.Player.PlayerId;
        }

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

        bool isKnownSession = _playerIdsBySessionId.TryGetValue(sessionId, out int playerId);
        bool hasSession = _sessionsByPlayerId.TryGetValue(playerId, out PlayerSession<TCallback> session);
        bool wasFound = isKnownSession
            && hasSession
            && string.Equals(session.SessionId, sessionId, StringComparison.Ordinal);

        player = wasFound ? session.Player : PlayerIdentity.Empty;
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
            ForgetSessionId(removedSession);
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

        if (wasRemoved)
        {
            ForgetSessionId(session);
        }

        return wasRemoved;
    }

    private void ForgetSessionId(PlayerSession<TCallback> session)
    {
        ICollection<KeyValuePair<string, int>> entries = _playerIdsBySessionId;
        KeyValuePair<string, int> entry = new KeyValuePair<string, int>(session.SessionId, session.Player.PlayerId);

        entries.Remove(entry);
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
