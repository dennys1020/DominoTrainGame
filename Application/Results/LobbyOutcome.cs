using System;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Results;

public sealed class LobbyOutcome<TStatus>
    where TStatus : struct, Enum
{
    public LobbyOutcome(TStatus status, LobbyState lobby)
    {
        Status = status;
        Lobby = lobby ?? throw new ArgumentNullException(nameof(lobby));
    }

    public TStatus Status { get; }

    public LobbyState Lobby { get; }
}
