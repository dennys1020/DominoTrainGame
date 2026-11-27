using System;
using System.Collections.Generic;
using DominoTrainGame.Shared.Rules;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyResult
{
    private const int EmptyMaximumPlayers = 0;

    static LobbyResult()
    {
        Empty = CreateFailure(LobbyActionResult.LobbyNotFound);
    }

    public LobbyResult(
        LobbyActionResult status,
        string code,
        int maximumPlayers,
        int highestDouble,
        LobbyRoster roster)
    {
        Status = status;
        Code = code ?? string.Empty;
        MaximumPlayers = maximumPlayers;
        HighestDouble = highestDouble;
        Roster = roster ?? throw new ArgumentNullException(nameof(roster));
    }

    public static LobbyResult Empty { get; }

    public LobbyActionResult Status { get; }

    public string Code { get; }

    public int MaximumPlayers { get; }

    public int HighestDouble { get; }

    public LobbyRoster Roster { get; }

    public long Version => Roster.Version;

    public int HostPlayerId => Roster.HostPlayerId;

    public IReadOnlyList<LobbyPlayerResult> Players => Roster.Players;

    public static LobbyResult CreateFailure(LobbyActionResult status)
    {
        LobbyResult failure = new LobbyResult(
            status,
            string.Empty,
            EmptyMaximumPlayers,
            MatchRules.DoubleTwelvePips,
            LobbyRoster.Empty);
        return failure;
    }

    public LobbyResult WithRoster(LobbyRoster roster)
    {
        LobbyResult lobby = new LobbyResult(Status, Code, MaximumPlayers, HighestDouble, roster);
        return lobby;
    }

    public LobbyResult WithDominoSet(int highestDouble, int maximumPlayers, LobbyRoster roster)
    {
        LobbyResult lobby = new LobbyResult(Status, Code, maximumPlayers, highestDouble, roster);
        return lobby;
    }
}
