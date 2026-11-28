using System;
using System.Collections.Generic;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyRoster
{

    static LobbyRoster()
    {
        Empty = new LobbyRoster(StateIdentifiers.EmptyVersion, StateIdentifiers.EmptyPlayerId, Array.Empty<LobbyPlayerResult>());
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
