using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class LobbyRoster
{
    private const long EmptyVersion = 0;
    private const long VersionStep = 1;
    private const int NoPlayers = 0;
    private const int HostIndex = 0;
    private const bool InitialReadyState = false;

    static LobbyRoster()
    {
        Empty = new LobbyRoster(EmptyVersion, new List<LobbyPlayerState>());
    }

    public LobbyRoster(long version, IReadOnlyList<LobbyPlayerState> players)
    {
        if (players == null)
        {
            throw new ArgumentNullException(nameof(players));
        }

        Version = version;
        Players = new List<LobbyPlayerState>(players).AsReadOnly();
    }

    public static LobbyRoster Empty { get; }

    public long Version { get; }

    public IReadOnlyList<LobbyPlayerState> Players { get; }

    public int Count => Players.Count;

    public bool AreAllPlayersReady => Players.All(player => player.IsReady);

    public bool HasPlayers => Players.Count > NoPlayers;

    public int HostPlayerId => HasPlayers ? Players[HostIndex].PlayerId : PlayerIdentity.EmptyPlayerId;

    public bool Contains(int playerId)
    {
        bool wasFound = Players.Any(player => player.PlayerId == playerId);
        return wasFound;
    }

    public LobbyRoster WithPlayer(PlayerIdentity member)
    {
        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        List<LobbyPlayerState> players = new List<LobbyPlayerState>(Players)
        {
            new LobbyPlayerState(member, InitialReadyState)
        };
        LobbyRoster roster = new LobbyRoster(Version + VersionStep, players);

        return roster;
    }

    public LobbyRoster WithoutPlayer(int playerId)
    {
        List<LobbyPlayerState> players = Players
            .Where(player => player.PlayerId != playerId)
            .ToList();
        LobbyRoster roster = new LobbyRoster(Version + VersionStep, players);

        return roster;
    }

    public LobbyRoster WithReadyState(int playerId, bool isReady)
    {
        List<LobbyPlayerState> players = new List<LobbyPlayerState>();

        foreach (LobbyPlayerState player in Players)
        {
            LobbyPlayerState updatedPlayer = player.PlayerId == playerId
                ? player.WithReadyState(isReady)
                : player;

            players.Add(updatedPlayer);
        }

        LobbyRoster roster = new LobbyRoster(Version + VersionStep, players);
        return roster;
    }

    public LobbyRoster WithNextVersion()
    {
        LobbyRoster roster = new LobbyRoster(Version + VersionStep, Players);
        return roster;
    }

    public LobbyPlayerState FindPlayer(int playerId)
    {
        LobbyPlayerState player = Players.FirstOrDefault(candidate => candidate.PlayerId == playerId)
            ?? LobbyPlayerState.Empty;
        return player;
    }

    public List<int> GetPlayerIds()
    {
        List<int> playerIds = Players.Select(player => player.PlayerId).ToList();
        return playerIds;
    }

    public List<int> GetPlayerIdsExcept(int excludedPlayerId)
    {
        List<int> playerIds = Players
            .Where(player => player.PlayerId != excludedPlayerId)
            .Select(player => player.PlayerId)
            .ToList();
        return playerIds;
    }
}
