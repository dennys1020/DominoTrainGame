using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Service.Contracts;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class SocialSessionRegistry : IPresenceTracker
{
    private readonly ConcurrentDictionary<int, SocialSession> _sessionsByPlayerId;

    public SocialSessionRegistry()
    {
        _sessionsByPlayerId = new ConcurrentDictionary<int, SocialSession>();
    }

    public void Register(SocialSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        _sessionsByPlayerId[session.Player.PlayerId] = session;
    }

    public bool IsOnline(int playerId)
    {
        bool isOnline = _sessionsByPlayerId.ContainsKey(playerId);
        return isOnline;
    }

    public IReadOnlyList<ISocialClientCallback> FindCallbacks(int playerId)
    {
        List<ISocialClientCallback> callbacks = new List<ISocialClientCallback>();
        bool isConnected = _sessionsByPlayerId.TryGetValue(playerId, out SocialSession session);

        if (isConnected)
        {
            callbacks.Add(session.Callback);
        }

        return callbacks;
    }

    public bool TryGetPlayer(string sessionId, out AuthenticatedPlayer player)
    {
        List<SocialSession> matchingSessions = _sessionsByPlayerId.Values
            .Where(session => string.Equals(session.SessionId, sessionId, StringComparison.Ordinal))
            .ToList();
        bool isFound = matchingSessions.Any();

        player = isFound ? matchingSessions.First().Player : AuthenticatedPlayer.Empty;
        return isFound;
    }

    public bool Remove(SocialSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        ICollection<KeyValuePair<int, SocialSession>> entries = _sessionsByPlayerId;
        KeyValuePair<int, SocialSession> entry = new KeyValuePair<int, SocialSession>(
            session.Player.PlayerId,
            session);
        bool wasRemoved = entries.Remove(entry);

        return wasRemoved;
    }
}
