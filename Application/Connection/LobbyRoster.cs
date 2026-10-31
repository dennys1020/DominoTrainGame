// Application/Connection/LobbyRoster.cs
using System;
using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Connection;

public sealed class LobbyRoster
{
    private const long EmptyVersion = 0;
    private const long VersionStep = 1;
    private const int NoPlayers = 0;
    private const int HostIndex = 0;
    private const int NoHostPlayerId = 0;

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

    public int HostPlayerId
    {
        get
        {
            int hostPlayerId = NoHostPlayerId;

            if (Players.Count > NoPlayers)
            {
                hostPlayerId = Players[HostIndex].PlayerId;
            }

            return hostPlayerId;
        }
    }

    public bool Contains(int playerId)
    {
        bool wasFound = false;

        foreach (LobbyPlayerState player in Players)
        {
            if (player.PlayerId == playerId)
            {
                wasFound = true;
            }
        }

        return wasFound;
    }

    public LobbyRoster WithPlayer(LobbyMember member)
    {
        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        List<LobbyPlayerState> players = new List<LobbyPlayerState>(Players);
        bool isHost = players.Count == NoPlayers;

        players.Add(new LobbyPlayerState(member.PlayerId, member.Username, isHost));

        LobbyRoster roster = new LobbyRoster(Version + VersionStep, players);
        return roster;
    }

    public LobbyRoster WithoutPlayer(int playerId)
    {
        List<LobbyPlayerState> players = new List<LobbyPlayerState>();

        foreach (LobbyPlayerState player in Players)
        {
            if (player.PlayerId != playerId)
            {
                bool isHost = players.Count == NoPlayers;
                players.Add(new LobbyPlayerState(player.PlayerId, player.Username, isHost));
            }
        }

        LobbyRoster roster = new LobbyRoster(Version + VersionStep, players);
        return roster;
    }

    public List<int> GetPlayerIdsExcept(int excludedPlayerId)
    {
        List<int> playerIds = new List<int>();

        foreach (LobbyPlayerState player in Players)
        {
            if (player.PlayerId != excludedPlayerId)
            {
                playerIds.Add(player.PlayerId);
            }
        }

        return playerIds;
    }
}
