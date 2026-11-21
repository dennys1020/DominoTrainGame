using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class FinishedMatchRecord
{
    public FinishedMatchRecord(string lobbyCode, IReadOnlyList<PlayerMatchResult> playerResults)
    {
        LobbyCode = lobbyCode ?? throw new ArgumentNullException(nameof(lobbyCode));
        PlayerResults = playerResults ?? throw new ArgumentNullException(nameof(playerResults));
    }

    public string LobbyCode { get; }

    public IReadOnlyList<PlayerMatchResult> PlayerResults { get; }
}
