using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyResult
{
    private const int EmptyMaximumPlayers = 0;

    static LobbyResult()
    {
        Empty = CreateFailure(LobbyActionResult.LobbyNotFound);
    }

    public LobbyResult(LobbyActionResult status, string code, int maximumPlayers, LobbyRoster roster)
    {
        Status = status;
        Code = code ?? string.Empty;
        MaximumPlayers = maximumPlayers;
        Roster = roster ?? throw new ArgumentNullException(nameof(roster));
    }

    public static LobbyResult Empty { get; }

    public LobbyActionResult Status { get; }

    public string Code { get; }

    public int MaximumPlayers { get; }

    public LobbyRoster Roster { get; }

    public long Version => Roster.Version;

    public int HostPlayerId => Roster.HostPlayerId;

    public IReadOnlyList<LobbyPlayerResult> Players => Roster.Players;

    public static LobbyResult CreateFailure(LobbyActionResult status)
    {
        LobbyResult failure = new LobbyResult(status, string.Empty, EmptyMaximumPlayers, LobbyRoster.Empty);
        return failure;
    }

    public LobbyResult WithRoster(LobbyRoster roster)
    {
        LobbyResult lobby = new LobbyResult(Status, Code, MaximumPlayers, roster);
        return lobby;
    }
}
