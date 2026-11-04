using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyResult
{
    static LobbyResult()
    {
        Empty = new LobbyResult(LobbyResultStatus.LobbyNotFound, LobbySettings.Empty, LobbyRoster.Empty);
    }

    public LobbyResult(LobbyResultStatus status, LobbySettings settings, LobbyRoster roster)
    {
        Status = status;
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Roster = roster ?? throw new ArgumentNullException(nameof(roster));
    }

    public static LobbyResult Empty { get; }

    public LobbyResultStatus Status { get; }

    public LobbySettings Settings { get; }

    public LobbyRoster Roster { get; }

    public string Code => Settings.Code;

    public int MaximumPlayers => Settings.MaximumPlayers;

    public long Version => Roster.Version;

    public int HostPlayerId => Roster.HostPlayerId;

    public IReadOnlyList<LobbyPlayerResult> Players => Roster.Players;

    public LobbyResult WithRoster(LobbyRoster roster)
    {
        LobbyResult lobby = new LobbyResult(Status, Settings, roster);
        return lobby;
    }
}
