using System;
using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class MatchState
{
    private const int FirstRoundEnginePips = Tile.MaximumPips;

    private readonly List<int> _playerIds;

    static MatchState()
    {
        Empty = new MatchState();
    }

    public MatchState(IReadOnlyList<int> playerIds, IReadOnlyList<Tile> shuffledTiles)
    {
        if (playerIds == null)
        {
            throw new ArgumentNullException(nameof(playerIds));
        }

        bool hasValidPlayerCount = playerIds.Count >= LobbyState.MinimumPlayerCount
            && playerIds.Count <= LobbyState.MaximumPlayerCount;

        if (!hasValidPlayerCount)
        {
            throw new ArgumentOutOfRangeException(nameof(playerIds));
        }

        _playerIds = new List<int>(playerIds);
        SyncRoot = new object();
        CurrentRound = new RoundState(_playerIds, shuffledTiles, FirstRoundEnginePips);
    }

    private MatchState()
    {
        _playerIds = new List<int>();
        SyncRoot = new object();
        CurrentRound = RoundState.Empty;
    }

    public static MatchState Empty { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);

    public object SyncRoot { get; }

    public IReadOnlyList<int> PlayerIds => _playerIds;

    public RoundState CurrentRound { get; }
}
