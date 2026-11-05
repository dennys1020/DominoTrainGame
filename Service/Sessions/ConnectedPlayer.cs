using System;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Service.Contracts;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class ConnectedPlayer
{
    public ConnectedPlayer(
        string sessionId,
        AuthenticatedPlayer player,
        IGameClientCallback callback)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public string SessionId { get; }

    public AuthenticatedPlayer Player { get; }

    public IGameClientCallback Callback { get; }
}
