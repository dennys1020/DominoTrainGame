using System;
using System.Collections.Generic;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.State;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class MatchUseCases : IMatchUseCases
{
    private static readonly ITraceLog _logger;

    private readonly MatchRegistry _matchRegistry;
    private readonly ITileShuffler _tileShuffler;
    private readonly IGameNotifier _gameNotifier;

    static MatchUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(MatchUseCases));
    }

    public MatchUseCases(MatchRegistry matchRegistry, ITileShuffler tileShuffler, IGameNotifier gameNotifier)
    {
        _matchRegistry = matchRegistry ?? throw new ArgumentNullException(nameof(matchRegistry));
        _tileShuffler = tileShuffler ?? throw new ArgumentNullException(nameof(tileShuffler));
        _gameNotifier = gameNotifier ?? throw new ArgumentNullException(nameof(gameNotifier));
    }

    public bool StartMatch(string code, IReadOnlyList<int> playerIds)
    {
        IReadOnlyList<Tile> shuffledTiles = _tileShuffler.Shuffle(Tile.CreateDoubleTwelveSet());
        MatchState match = new MatchState(playerIds, shuffledTiles);
        bool wasStarted = _matchRegistry.TryAdd(code, match);

        if (wasStarted)
        {
            _logger.InfoFormat("A match started. MatchCode: {0}.", code);
            NotifyUpdates(match, match.PlayerIds, match.PlayerIds);
        }
        else
        {
            _logger.WarnFormat(
                "A match start was rejected because the code or a player is already in a match. MatchCode: {0}.",
                code);
        }

        return wasStarted;
    }

    public PlayTileStatus PlayTile(int playerId, Tile tile, int trainOwnerId)
    {
        if (tile == null)
        {
            throw new ArgumentNullException(nameof(tile));
        }

        PlayTileStatus status = PlayTileStatus.MatchNotFound;
        MatchState match = _matchRegistry.FindByPlayer(playerId);

        if (!match.IsEmpty)
        {
            lock (match.SyncRoot)
            {
                status = match.PlayTile(playerId, tile, trainOwnerId);
            }
        }

        if (status == PlayTileStatus.Success)
        {
            NotifyUpdates(match, match.PlayerIds, new List<int> { playerId });
        }
        else
        {
            _logger.InfoFormat("A tile play was rejected. PlayerId: {0}. Status: {1}.", playerId, status);
        }

        return status;
    }

    public DrawTileStatus DrawTile(int playerId)
    {
        DrawTileStatus status = DrawTileStatus.MatchNotFound;
        MatchState match = _matchRegistry.FindByPlayer(playerId);

        if (!match.IsEmpty)
        {
            lock (match.SyncRoot)
            {
                status = match.DrawTile(playerId);
            }
        }

        if (status == DrawTileStatus.Success)
        {
            NotifyUpdates(match, match.PlayerIds, new List<int> { playerId });
        }
        else
        {
            _logger.InfoFormat("A tile draw was rejected. PlayerId: {0}. Status: {1}.", playerId, status);
        }

        return status;
    }

    public PassTurnStatus PassTurn(int playerId)
    {
        PassTurnStatus status = PassTurnStatus.MatchNotFound;
        MatchState match = _matchRegistry.FindByPlayer(playerId);

        if (!match.IsEmpty)
        {
            lock (match.SyncRoot)
            {
                status = match.PassTurn(playerId);
            }
        }

        if (status == PassTurnStatus.Success)
        {
            NotifyUpdates(match, match.PlayerIds, new List<int>());
        }
        else
        {
            _logger.InfoFormat("A turn pass was rejected. PlayerId: {0}. Status: {1}.", playerId, status);
        }

        return status;
    }

    public bool SendMatchState(int playerId)
    {
        MatchState match = _matchRegistry.FindByPlayer(playerId);
        bool isInMatch = !match.IsEmpty;

        if (isInMatch)
        {
            List<int> requesterIds = new List<int> { playerId };
            NotifyUpdates(match, requesterIds, requesterIds);
        }
        else
        {
            _logger.InfoFormat(
                "A match state request was rejected because the player is not in a match. PlayerId: {0}.",
                playerId);
        }

        return isInMatch;
    }

    private static MatchUpdatedNotice CreateMatchUpdatedNotice(MatchState match, IReadOnlyList<int> recipientPlayerIds)
    {
        MatchUpdatedNotice notice = new MatchUpdatedNotice(
            recipientPlayerIds,
            match.CurrentPlayerId,
            match.Boneyard.Count,
            match.EngineTile,
            CountTilesByPlayer(match),
            CreateTrainSnapshots(match));
        return notice;
    }

    private static Dictionary<int, int> CountTilesByPlayer(MatchState match)
    {
        Dictionary<int, int> tileCountsByPlayerId = new Dictionary<int, int>();

        foreach (int playerId in match.PlayerIds)
        {
            tileCountsByPlayerId.Add(playerId, match.GetHand(playerId).Count);
        }

        return tileCountsByPlayerId;
    }

    private static List<TrainSnapshot> CreateTrainSnapshots(MatchState match)
    {
        List<TrainSnapshot> trains = new List<TrainSnapshot>();

        foreach (Train train in match.Trains)
        {
            trains.Add(new TrainSnapshot(train.OwnerId, train.IsOpen, train.Tiles));
        }

        return trains;
    }

    private static HandUpdatedNotice CreateHandUpdatedNotice(MatchState match, int playerId)
    {
        HandUpdatedNotice notice = new HandUpdatedNotice(playerId, match.GetHand(playerId));
        return notice;
    }

    private void NotifyUpdates(
        MatchState match,
        IReadOnlyList<int> boardRecipientIds,
        IReadOnlyList<int> handOwnerIds)
    {
        MatchUpdatedNotice matchNotice;
        List<HandUpdatedNotice> handNotices = new List<HandUpdatedNotice>();

        lock (match.SyncRoot)
        {
            matchNotice = CreateMatchUpdatedNotice(match, boardRecipientIds);

            foreach (int handOwnerId in handOwnerIds)
            {
                handNotices.Add(CreateHandUpdatedNotice(match, handOwnerId));
            }
        }

        _gameNotifier.NotifyMatchUpdated(matchNotice);

        foreach (HandUpdatedNotice handNotice in handNotices)
        {
            _gameNotifier.NotifyHandUpdated(handNotice);
        }
    }
}
