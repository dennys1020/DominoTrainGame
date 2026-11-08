using System;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class PlayerSession<TCallback>
    where TCallback : class
{
    public PlayerSession(string sessionId, PlayerIdentity player, TCallback callback)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public string SessionId { get; }

    public PlayerIdentity Player { get; }

    public TCallback Callback { get; }
}
