using System;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class SocialSession
{
    public SocialSession(string sessionId, string sessionToken, AuthenticatedPlayer player)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        SessionToken = sessionToken ?? throw new ArgumentNullException(nameof(sessionToken));
        Player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public string SessionId { get; }

    public string SessionToken { get; }

    public AuthenticatedPlayer Player { get; }
}
