using System;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Service.Contracts;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class SocialSession
{
    public SocialSession(string sessionId, AuthenticatedPlayer player, ISocialClientCallback callback)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public string SessionId { get; }

    public AuthenticatedPlayer Player { get; }

    public ISocialClientCallback Callback { get; }
}
