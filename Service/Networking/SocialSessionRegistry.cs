using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Authentication;

namespace DominoTrainGame.Server.Service.Networking;

public sealed class SocialSessionRegistry
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
