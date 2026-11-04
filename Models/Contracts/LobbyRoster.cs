using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyRoster
{
    private const long EmptyVersion = 0;
    private const int NoHostPlayerId = 0;

    static LobbyRoster()
    {
        Empty = new LobbyRoster(EmptyVersion, NoHostPlayerId, Array.Empty<LobbyPlayerResult>());
    }

    public LobbyRoster(long version, int hostPlayerId, IReadOnlyList<LobbyPlayerResult> players)
    {
        if (players == null)
        {
            throw new ArgumentNullException(nameof(players));
        }

        List<LobbyPlayerResult> copiedPlayers = new List<LobbyPlayerResult>(players);

        Version = version;
        HostPlayerId = hostPlayerId;
        Players = copiedPlayers.AsReadOnly();
    }

    public static LobbyRoster Empty { get; }

    public long Version { get; }

    public int HostPlayerId { get; }

    public IReadOnlyList<LobbyPlayerResult> Players { get; }
}
