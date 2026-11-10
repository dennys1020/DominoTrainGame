using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class MatchState
{
    private const int EnginePips = Tile.MaximumPips;
    private const int HandSize = 15;
    private const int MinimumPlayers = 2;
    private const int MaximumPlayers = 4;
    private const int EngineTileCount = 1;
    private const int FirstPlayerIndex = 0;
    private const int TurnStep = 1;

    private readonly List<int> _playerIds;
    private readonly Dictionary<int, List<Tile>> _handsByPlayerId;
    private readonly Dictionary<int, Train> _trainsByOwnerId;
    private int _currentTurnIndex;

    public MatchState(IReadOnlyList<int> playerIds, IReadOnlyList<Tile> shuffledTiles)
    {
        if (playerIds == null)
        {
            throw new ArgumentNullException(nameof(playerIds));
        }

        if (shuffledTiles == null)
        {
            throw new ArgumentNullException(nameof(shuffledTiles));
        }

        bool hasValidPlayerCount = playerIds.Count >= MinimumPlayers && playerIds.Count <= MaximumPlayers;

        if (!hasValidPlayerCount)
        {
            throw new ArgumentOutOfRangeException(nameof(playerIds));
        }

        List<Tile> remainingTiles = shuffledTiles.Where(tile => !IsEngineTile(tile)).ToList();
        bool hasSingleEngineTile = shuffledTiles.Count - remainingTiles.Count == EngineTileCount;

        if (!hasSingleEngineTile)
        {
            throw new ArgumentException(
                "The tile set must contain the engine tile exactly once.",
                nameof(shuffledTiles));
        }

        _playerIds = new List<int>(playerIds);
        _handsByPlayerId = new Dictionary<int, List<Tile>>();
        _trainsByOwnerId = new Dictionary<int, Train>();
        EngineTile = new Tile(EnginePips, EnginePips);

        DealHands(remainingTiles);
        Boneyard = new Boneyard(remainingTiles.Skip(_playerIds.Count * HandSize));
        CreateTrains();
    }

    public Tile EngineTile { get; }

    public Boneyard Boneyard { get; }

    public int CurrentPlayerId => _playerIds[_currentTurnIndex];

    public bool TryGetHand(int playerId, out IReadOnlyList<Tile> hand)
    {
        bool wasFound = _handsByPlayerId.TryGetValue(playerId, out List<Tile> foundHand);
        hand = wasFound ? foundHand : new List<Tile>();
        return wasFound;
    }

    public bool TryGetTrain(int ownerId, out Train train)
    {
        bool wasFound = _trainsByOwnerId.TryGetValue(ownerId, out Train foundTrain);
        train = wasFound ? foundTrain : Train.Empty;
        return wasFound;
    }

    public void AdvanceTurn()
    {
        _currentTurnIndex = (_currentTurnIndex + TurnStep) % _playerIds.Count;
    }

    private static bool IsEngineTile(Tile tile)
    {
        bool isEngineTile = tile.IsDouble && tile.LeftPips == EnginePips;
        return isEngineTile;
    }

    private void DealHands(List<Tile> tiles)
    {
        for (int playerIndex = FirstPlayerIndex; playerIndex < _playerIds.Count; playerIndex++)
        {
            List<Tile> hand = tiles.GetRange(playerIndex * HandSize, HandSize);
            _handsByPlayerId.Add(_playerIds[playerIndex], hand);
        }
    }

    private void CreateTrains()
    {
        foreach (int playerId in _playerIds)
        {
            _trainsByOwnerId.Add(playerId, new Train(playerId, EnginePips));
        }

        _trainsByOwnerId.Add(Train.MexicanTrainOwnerId, new Train(Train.MexicanTrainOwnerId, EnginePips));
    }
}
