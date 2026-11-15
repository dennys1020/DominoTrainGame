using System;
using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class MatchState
{
    private const int FirstRoundEnginePips = Tile.MaximumPips;
    private const int EnginePipsStep = 1;
    private const int InitialScore = 0;

    private readonly List<int> _playerIds;
    private readonly Dictionary<int, int> _scoresByPlayerId = new Dictionary<int, int>();
    private RoundState _lastScoredRound = RoundState.Empty;

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
        AddInitialScores();
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

    public RoundState CurrentRound { get; private set; }

    public IReadOnlyDictionary<int, int> ScoresByPlayerId => _scoresByPlayerId;

    public bool HasEnoughPlayers => _playerIds.Count >=
        LobbyState.MinimumPlayerCount;

    public bool HasNextRound => CurrentRound.EngineTile.LeftPips > Tile.MinimumPips;

    public bool IsFinished => CurrentRound.IsFinished && !HasNextRound;

    public bool RemovePlayer(int playerId)
    {
        bool wasRemoved = _playerIds.Remove(playerId);

        if (wasRemoved)
        {
            CurrentRound.RemovePlayer(playerId);
        }

        return wasRemoved;
    }

    public bool StartNextRound(IReadOnlyList<Tile> shuffledTiles)
    {
        if (shuffledTiles == null)
        {
            throw new ArgumentNullException(nameof(shuffledTiles));
        }

        bool canStartNextRound = CurrentRound.IsFinished && HasNextRound;

        if (canStartNextRound)
        {
            int nextEnginePips = CurrentRound.EngineTile.LeftPips - EnginePipsStep;
            CurrentRound = new RoundState(_playerIds, shuffledTiles, nextEnginePips);
        }

        return canStartNextRound;
    }

    public void ScoreFinishedRound()
    {
        bool canScoreRound = CurrentRound.IsFinished && !ReferenceEquals(CurrentRound, _lastScoredRound);

        if (canScoreRound)
        {
            foreach (int playerId in _playerIds)
            {
                _scoresByPlayerId[playerId] += CountHandPips(CurrentRound.GetHand(playerId));
            }

            _lastScoredRound = CurrentRound;
        }
    }

    private static int CountHandPips(IReadOnlyList<Tile> hand)
    {
        int handPips = InitialScore;

        foreach (Tile tile in hand)
        {
            handPips += tile.PipCount;
        }

        return handPips;
    }

    private void AddInitialScores()
    {
        foreach (int playerId in _playerIds)
        {
            _scoresByPlayerId.Add(playerId, InitialScore);
        }
    }
}
