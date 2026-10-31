using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyResult
{
    public LobbyResult(
        LobbyResultStatus status,
        string code,
        int maximumPlayers,
        long version,
        int hostPlayerId,
        IReadOnlyList<LobbyPlayerResult> players)
    {
        if (players == null)
        {
            throw new ArgumentNullException(nameof(players));
        }

        List<LobbyPlayerResult> copiedPlayers =
            new List<LobbyPlayerResult>(players);

        Status = status;
        Code = code ?? string.Empty;
        MaximumPlayers = maximumPlayers;
        Version = version;
        HostPlayerId = hostPlayerId;
        Players = copiedPlayers.AsReadOnly();
    }

    public LobbyResultStatus Status { get; }

    public string Code { get; }

    public int MaximumPlayers { get; }

    public long Version { get; }

    public int HostPlayerId { get; }

    public IReadOnlyList<LobbyPlayerResult> Players { get; }
}
