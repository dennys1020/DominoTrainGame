using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class RoundState
{
    private const int TurnStep = 1;
    private const int FirstIndex = 0;
    private const int TileNotFoundIndex = -1;

    private readonly List<int> _playerIds;
    private readonly Dictionary<int, List<Tile>> _handsByPlayerId;
    private readonly Dictionary<int, Train> _trainsByOwnerId;
    private readonly HashSet<int> _consecutivePasserIds = new HashSet<int>();
    private int _currentTurnIndex;
    private bool _hasDrawnThisTurn;

    static RoundState()
    {
        Empty = new RoundState();
    }

    public RoundState(IReadOnlyList<int> playerIds, IReadOnlyList<Tile> shuffledTiles, int enginePips, int handSize)
    {
        if (playerIds == null)
        {
            throw new ArgumentNullException(nameof(playerIds));
        }

        if (shuffledTiles == null)
        {
            throw new ArgumentNullException(nameof(shuffledTiles));
        }

        EngineTile = new Tile(enginePips, enginePips);
        List<Tile> remainingTiles = GetTilesWithoutEngine(shuffledTiles);
        bool hasSingleEngineTile = HasSingleEngineTile(shuffledTiles);

        if (!hasSingleEngineTile)
        {
            throw new ArgumentException(
                "The tile set must contain the engine tile exactly once.",
                nameof(shuffledTiles));
        }

        bool hasEnoughTiles = playerIds.Count * handSize <= remainingTiles.Count;

        if (!hasEnoughTiles)
        {
            throw new ArgumentOutOfRangeException(
                nameof(handSize),
                "The tile set does not have enough tiles to deal that hand size.");
        }

        _playerIds = new List<int>(playerIds);
        _handsByPlayerId = new Dictionary<int, List<Tile>>();
        _trainsByOwnerId = new Dictionary<int, Train>();

        DealHands(remainingTiles, handSize);
        Boneyard = new Boneyard(remainingTiles.Skip(_playerIds.Count * handSize));
        CreateTrains();
    }

    private RoundState()
    {
        _playerIds = new List<int>();
        _handsByPlayerId = new Dictionary<int, List<Tile>>();
        _trainsByOwnerId = new Dictionary<int, Train>();
        EngineTile = Tile.Empty;
        Boneyard = new Boneyard(new List<Tile>());
    }

    public static RoundState Empty { get; }

    public Tile EngineTile { get; }

    public Boneyard Boneyard { get; }

    public IReadOnlyCollection<Train> Trains => _trainsByOwnerId.Values;

    public int CurrentPlayerId => _playerIds[_currentTurnIndex];

    public RoundEndReason EndReason { get; private set; } = RoundEndReason.InProgress;

    public bool IsFinished => EndReason != RoundEndReason.InProgress;

    private List<Tile> CurrentHand => _handsByPlayerId[CurrentPlayerId];

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

        List<Tile> currentHand = CurrentHand;
        int tileIndex = FindTileIndex(currentHand, tile);
        Train train = FindTrain(trainOwnerId);
        PlayTileStatus status = ValidatePlay(playerId, tileIndex, train);

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
            CurrentHand.Add(drawnTile);
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
            RegisterPass();
            AdvanceTurn();
        }

        return status;
    }

    public void RemovePlayer(int playerId)
    {
        if (!_playerIds.Contains(playerId))
        {
            throw new ArgumentException("The player is not part of the round.", nameof(playerId));
        }

        int leavingIndex = _playerIds.IndexOf(playerId);
        bool wasPlayerTurn = leavingIndex == _currentTurnIndex;

        Boneyard.ReturnTiles(_handsByPlayerId[playerId]);
        _playerIds.RemoveAt(leavingIndex);
        _handsByPlayerId.Remove(playerId);
        _consecutivePasserIds.Remove(playerId);
        _trainsByOwnerId[playerId].Open();
        AdjustTurnAfterRemoval(leavingIndex, wasPlayerTurn);
    }

    private void AdjustTurnAfterRemoval(int leavingIndex, bool wasPlayerTurn)
    {
        bool hasPlayersLeft = _playerIds.Any();

        if (leavingIndex < _currentTurnIndex)
        {
            _currentTurnIndex -= TurnStep;
        }
        else if (wasPlayerTurn && hasPlayersLeft)
        {
            _currentTurnIndex %= _playerIds.Count;
            _hasDrawnThisTurn = false;
        }

    }

    private PlayTileStatus ValidatePlay(int playerId, int tileIndex, Train train)
    {
        bool isPlayerTurn = playerId == CurrentPlayerId;
        bool hasTile = tileIndex != TileNotFoundIndex;
        bool isOpenDoubleTrain = train.HasOpenDouble;
        bool mustSatisfyDouble = !isOpenDoubleTrain && HasAnyOpenDouble();
        bool canUseTrain = isOpenDoubleTrain || train.CanBePlayedBy(playerId);
        PlayTileStatus status = PlayTileStatus.Success;

        if (!isPlayerTurn)
        {
            status = PlayTileStatus.NotYourTurn;
        }
        else if (!hasTile)
        {
            status = PlayTileStatus.TileNotInHand;
        }
        else if (mustSatisfyDouble)
        {
            status = PlayTileStatus.MustSatisfyDouble;
        }
        else if (!canUseTrain)
        {
            status = PlayTileStatus.TrainNotAvailable;
        }
        else if (!train.CanAccept(CurrentHand[tileIndex]))
        {
            status = PlayTileStatus.TileDoesNotMatch;
        }

        return status;
    }

    private bool HasAnyOpenDouble()
    {
        bool hasAnyOpenDouble = false;

        foreach (Train train in _trainsByOwnerId.Values)
        {
            if (train.HasOpenDouble)
            {
                hasAnyOpenDouble = true;
            }
        }

        return hasAnyOpenDouble;
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

    private void DealHands(List<Tile> tiles, int handSize)
    {
        for (int playerIndex = FirstIndex; playerIndex < _playerIds.Count; playerIndex++)
        {
            List<Tile> hand = tiles.GetRange(playerIndex * handSize, handSize);
            _handsByPlayerId.Add(_playerIds[playerIndex], hand);
        }
    }

    private void CreateTrains()
    {
        int enginePips = EngineTile.LeftPips;

        foreach (int playerId in _playerIds)
        {
            _trainsByOwnerId.Add(playerId, new Train(playerId, enginePips));
        }

        _trainsByOwnerId.Add(Train.MexicanTrainOwnerId, new Train(Train.MexicanTrainOwnerId, enginePips));
    }

    private void ApplyPlay(List<Tile> hand, int tileIndex, Train train)
    {
        Tile playedTile = hand[tileIndex];
        bool isOwnTrain = train.OwnerId == CurrentPlayerId;

        train.AddTile(playedTile);
        hand.RemoveAt(tileIndex);
        _consecutivePasserIds.Clear();

        if (isOwnTrain)
        {
            train.Close();
        }

        if (!hand.Any())
        {
            EndReason = RoundEndReason.EmptyHand;
        }

        if (playedTile.IsDouble)
        {
            _hasDrawnThisTurn = false;
        }
        else
        {
            AdvanceTurn();
        }
    }

    private void RegisterPass()
    {
        if (Boneyard.HasTiles)
        {
            _consecutivePasserIds.Clear();
        }
        else
        {
            _consecutivePasserIds.Add(CurrentPlayerId);
        }

        if (_consecutivePasserIds.Count == _playerIds.Count)
        {
            EndReason = RoundEndReason.Blocked;
        }
    }
}
