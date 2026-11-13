using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class MatchState
{
    private const int EnginePips = Tile.MaximumPips;
    private const int HandSize = 15;
    private const int TurnStep = 1;
    private const int FirstIndex = 0;
    private const int TileNotFoundIndex = -1;

    private readonly List<int> _playerIds;
    private readonly Dictionary<int, List<Tile>> _handsByPlayerId;
    private readonly Dictionary<int, Train> _trainsByOwnerId;
    private int _currentTurnIndex;
    private bool _hasDrawnThisTurn;

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

        if (shuffledTiles == null)
        {
            throw new ArgumentNullException(nameof(shuffledTiles));
        }

        bool hasValidPlayerCount = playerIds.Count >= LobbyState.MinimumPlayerCount
            && playerIds.Count <= LobbyState.MaximumPlayerCount;

        if (!hasValidPlayerCount)
        {
            throw new ArgumentOutOfRangeException(nameof(playerIds));
        }

        EngineTile = new Tile(EnginePips, EnginePips);
        List<Tile> remainingTiles = GetTilesWithoutEngine(shuffledTiles);
        bool hasSingleEngineTile = HasSingleEngineTile(shuffledTiles);

        if (!hasSingleEngineTile)
        {
            throw new ArgumentException(
                "The tile set must contain the engine tile exactly once.",
                nameof(shuffledTiles));
        }

        _playerIds = new List<int>(playerIds);
        _handsByPlayerId = new Dictionary<int, List<Tile>>();
        _trainsByOwnerId = new Dictionary<int, Train>();
        SyncRoot = new object();

        DealHands(remainingTiles);
        Boneyard = new Boneyard(remainingTiles.Skip(_playerIds.Count * HandSize));
        CreateTrains();
    }

    private MatchState()
    {
        _playerIds = new List<int>();
        _handsByPlayerId = new Dictionary<int, List<Tile>>();
        _trainsByOwnerId = new Dictionary<int, Train>();
        EngineTile = Tile.Empty;
        Boneyard = new Boneyard(new List<Tile>());
        SyncRoot = new object();
    }

    public static MatchState Empty { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);

    public Tile EngineTile { get; }

    public Boneyard Boneyard { get; }

    public object SyncRoot { get; }

    public IReadOnlyList<int> PlayerIds => _playerIds;

    public IReadOnlyCollection<Train> Trains => _trainsByOwnerId.Values;

    public int CurrentPlayerId => _playerIds[_currentTurnIndex];

    public IReadOnlyList<Tile> GetHand(int playerId)
    {
        IReadOnlyList<Tile> hand = _handsByPlayerId[playerId];
        return hand;
    }

    public Train FindTrain(int ownerId)
    {
        Train train = _trainsByOwnerId.ContainsKey(ownerId) ? _trainsByOwnerId[ownerId] : Train.Empty;
        return train;
    }

    public PlayTileStatus PlayTile(int playerId, Tile tile, int trainOwnerId)
    {
        if (tile == null)
        {
            throw new ArgumentNullException(nameof(tile));
        }

        List<Tile> currentHand = _handsByPlayerId[CurrentPlayerId];
        bool isPlayerTurn = playerId == CurrentPlayerId;
        int tileIndex = FindTileIndex(currentHand, tile);
        bool hasTile = tileIndex != TileNotFoundIndex;
        Train train = FindTrain(trainOwnerId);
        bool canUseTrain = train.CanBePlayedBy(playerId);
        PlayTileStatus status = PlayTileStatus.Success;

        if (!isPlayerTurn)
        {
            status = PlayTileStatus.NotYourTurn;
        }
        else if (!hasTile)
        {
            status = PlayTileStatus.TileNotInHand;
        }
        else if (!canUseTrain)
        {
            status = PlayTileStatus.TrainNotAvailable;
        }
        else if (!train.CanAccept(currentHand[tileIndex]))
        {
            status = PlayTileStatus.TileDoesNotMatch;
        }

        if (status == PlayTileStatus.Success)
        {
            ApplyPlay(currentHand, tileIndex, train);
        }

        return status;
    }

    public DrawTileStatus DrawTile(int playerId)
    {
        bool isPlayerTurn = playerId == CurrentPlayerId;
        DrawTileStatus status = DrawTileStatus.Success;

        if (!isPlayerTurn)
        {
            status = DrawTileStatus.NotYourTurn;
        }
        else if (_hasDrawnThisTurn)
        {
            status = DrawTileStatus.AlreadyDrewTile;
        }
        else if (!Boneyard.HasTiles)
        {
            status = DrawTileStatus.BoneyardEmpty;
        }

        if (status == DrawTileStatus.Success)
        {
            Tile drawnTile = Boneyard.Draw();
            _handsByPlayerId[CurrentPlayerId].Add(drawnTile);
            _hasDrawnThisTurn = true;
        }

        return status;
    }

    public PassTurnStatus PassTurn(int playerId)
    {
        bool isPlayerTurn = playerId == CurrentPlayerId;
        bool canPass = _hasDrawnThisTurn || !Boneyard.HasTiles;
        PassTurnStatus status = PassTurnStatus.Success;

        if (!isPlayerTurn)
        {
            status = PassTurnStatus.NotYourTurn;
        }
        else if (!canPass)
        {
            status = PassTurnStatus.MustDrawFirst;
        }

        if (status == PassTurnStatus.Success)
        {
            _trainsByOwnerId[CurrentPlayerId].Open();
            AdvanceTurn();
        }

        return status;
    }

    private void AdvanceTurn()
    {
        _currentTurnIndex = (_currentTurnIndex + TurnStep) % _playerIds.Count;
        _hasDrawnThisTurn = false;
    }

    private bool HasSingleEngineTile(IReadOnlyList<Tile> tiles)
    {
        bool hasEngineTile = false;
        bool hasRepeatedEngineTile = false;

        foreach (Tile tile in tiles)
        {
            if (tile.IsSameTile(EngineTile))
            {
                hasRepeatedEngineTile = hasEngineTile;
                hasEngineTile = true;
            }
        }

        bool hasSingleEngineTile = hasEngineTile && !hasRepeatedEngineTile;
        return hasSingleEngineTile;
    }

    private List<Tile> GetTilesWithoutEngine(IReadOnlyList<Tile> tiles)
    {
        List<Tile> remainingTiles = new List<Tile>();

        foreach (Tile tile in tiles)
        {
            if (!tile.IsSameTile(EngineTile))
            {
                remainingTiles.Add(tile);
            }
        }

        return remainingTiles;
    }

    private void DealHands(List<Tile> tiles)
    {
        for (int playerIndex = FirstIndex; playerIndex < _playerIds.Count; playerIndex++)
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

    private static int FindTileIndex(List<Tile> hand, Tile tile)
    {
        int tileIndex = TileNotFoundIndex;

        for (int handIndex = FirstIndex; handIndex < hand.Count && tileIndex == TileNotFoundIndex; handIndex++)
        {
            if (hand[handIndex].IsSameTile(tile))
            {
                tileIndex = handIndex;
            }
        }

        return tileIndex;
    }

    private void ApplyPlay(List<Tile> hand, int tileIndex, Train train)
    {
        Tile playedTile = hand[tileIndex];
        bool isOwnTrain = train.OwnerId == CurrentPlayerId;

        train.AddTile(playedTile);
        hand.RemoveAt(tileIndex);

        if (isOwnTrain)
        {
            train.Close();
        }

        AdvanceTurn();
    }
}
