using System;
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
    private static readonly ITraceLog _logger;

    private readonly Dictionary<int, PlayerSession<TCallback>> _sessionsByPlayerId;
    private readonly Dictionary<string, int> _playerIdsBySessionId;
    private readonly object _sessionsLock;

    static PlayerSessionRegistry()
    {
        _logger = TraceLogManager.GetLogger(typeof(PlayerSessionRegistry<TCallback>));
    }

    public PlayerSessionRegistry()
    {
        _sessionsByPlayerId = new Dictionary<int, PlayerSession<TCallback>>();
        _playerIdsBySessionId = new Dictionary<string, int>(StringComparer.Ordinal);
        _sessionsLock = new object();
    }

    public bool TryAdd(PlayerSession<TCallback> session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        bool wasAdded;

        lock (_sessionsLock)
        {
            wasAdded = !_sessionsByPlayerId.ContainsKey(session.Player.PlayerId);

            if (wasAdded)
            {
                _sessionsByPlayerId.Add(session.Player.PlayerId, session);
                _playerIdsBySessionId[session.SessionId] = session.Player.PlayerId;
            }
        }

        return wasAdded;
    }

    public bool IsOnline(int playerId)
    {
        bool isOnline;

        lock (_sessionsLock)
        {
            isOnline = _sessionsByPlayerId.ContainsKey(playerId);
        }

        return isOnline;
    }

    public PlayerIdentity FindPlayer(string sessionId)
    {
        if (sessionId == null)
        {
            throw new ArgumentNullException(nameof(sessionId));
        }

        PlayerIdentity player = PlayerIdentity.Empty;

        lock (_sessionsLock)
        {
            if (IsCurrentSession(sessionId))
            {
                player = _sessionsByPlayerId[_playerIdsBySessionId[sessionId]].Player;
            }
        }

        return player;
    }

    public bool TrySend(int playerId, Action<TCallback> send)
    {
        if (send == null)
        {
            throw new ArgumentNullException(nameof(send));
        }

        List<TCallback> callbacks = FindCallbacks(playerId);

        foreach (TCallback callback in callbacks)
        {
            SendSafely(callback, send);
        }

        bool isConnected = callbacks.Any();
        return isConnected;
    }

    public bool TryDisconnect(int playerId)
    {
        List<PlayerSession<TCallback>> removedSessions = new List<PlayerSession<TCallback>>();

        lock (_sessionsLock)
        {
            if (_sessionsByPlayerId.ContainsKey(playerId))
            {
                PlayerSession<TCallback> removedSession = _sessionsByPlayerId[playerId];
                _sessionsByPlayerId.Remove(playerId);
                ForgetSessionId(removedSession);
                removedSessions.Add(removedSession);
            }
        }

        foreach (PlayerSession<TCallback> removedSession in removedSessions)
        {
            AbortChannel(removedSession.Callback);
        }

        bool wasRemoved = removedSessions.Any();
        return wasRemoved;
    }

    public bool Remove(PlayerSession<TCallback> session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        bool wasRemoved;

        lock (_sessionsLock)
        {
            wasRemoved = IsStoredSession(session);

            if (wasRemoved)
            {
                _sessionsByPlayerId.Remove(session.Player.PlayerId);
                ForgetSessionId(session);
            }
        }

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

    private List<TCallback> FindCallbacks(int playerId)
    {
        List<TCallback> callbacks = new List<TCallback>();

        lock (_sessionsLock)
        {
            if (_sessionsByPlayerId.ContainsKey(playerId))
            {
                callbacks.Add(_sessionsByPlayerId[playerId].Callback);
            }
        }

        return callbacks;
    }

    private bool IsCurrentSession(string sessionId)
    {
        bool isCurrentSession = false;

        if (_playerIdsBySessionId.ContainsKey(sessionId))
        {
            int playerId = _playerIdsBySessionId[sessionId];
            isCurrentSession = _sessionsByPlayerId.ContainsKey(playerId)
                && string.Equals(_sessionsByPlayerId[playerId].SessionId, sessionId, StringComparison.Ordinal);
        }

        return isCurrentSession;
    }

    private bool IsStoredSession(PlayerSession<TCallback> session)
    {
        int playerId = session.Player.PlayerId;
        bool isStoredSession = _sessionsByPlayerId.ContainsKey(playerId)
            && ReferenceEquals(_sessionsByPlayerId[playerId], session);
        return isStoredSession;
    }

    private void ForgetSessionId(PlayerSession<TCallback> session)
    {
        bool isMappedToPlayer = _playerIdsBySessionId.ContainsKey(session.SessionId)
            && _playerIdsBySessionId[session.SessionId] == session.Player.PlayerId;

        if (isMappedToPlayer)
        {
            _playerIdsBySessionId.Remove(session.SessionId);
        }
    }
}
