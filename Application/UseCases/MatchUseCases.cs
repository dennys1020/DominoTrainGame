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

    public bool StartMatch(string code, IReadOnlyList<int> playerIds, DominoSet dominoSet)
    {
        if (dominoSet == null)
        {
            throw new ArgumentNullException(nameof(dominoSet));
        }

        MatchState matchState = new MatchState(playerIds, dominoSet, ShuffleTiles(dominoSet));
        bool wasStarted = _matchRegistry.TryAdd(code, matchState);

        if (wasStarted)
        {
            _logger.InfoFormat("A match started. MatchCode: {0}. HighestDouble: {1}.", code, dominoSet.HighestDouble);
            NotifyUpdates(matchState, matchState.PlayerIds, matchState.PlayerIds);
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
        MatchState matchState = _matchRegistry.FindByPlayer(playerId);
        bool hasEndedRound = false;

        if (!matchState.IsEmpty)
        {
            lock (matchState.SyncRoot)
            {
                status = matchState.IsFinished
                    ? PlayTileStatus.MatchNotFound
                    : matchState.CurrentRound.PlayTile(playerId, tile, trainOwnerId);
                hasEndedRound = status == PlayTileStatus.Success && CloseRoundIfFinished(matchState);
            }
        }

        if (status == PlayTileStatus.Success)
        {
            NotifyTurnResult(matchState, new List<int> { playerId }, hasEndedRound);
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
        MatchState matchState = _matchRegistry.FindByPlayer(playerId);

        if (!matchState.IsEmpty)
        {
            lock (matchState.SyncRoot)
            {
                status = matchState.IsFinished
                    ? DrawTileStatus.MatchNotFound
                    : matchState.CurrentRound.DrawTile(playerId);
            }
        }

        if (status == DrawTileStatus.Success)
        {
            NotifyUpdates(matchState, matchState.PlayerIds, new List<int> { playerId });
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
        MatchState matchState = _matchRegistry.FindByPlayer(playerId);
        bool hasEndedRound = false;

        if (!matchState.IsEmpty)
        {
            lock (matchState.SyncRoot)
            {
                status = matchState.IsFinished
                    ? PassTurnStatus.MatchNotFound
                    : matchState.CurrentRound.PassTurn(playerId);
                hasEndedRound = status == PassTurnStatus.Success && CloseRoundIfFinished(matchState);
            }
        }

        if (status == PassTurnStatus.Success)
        {
            NotifyTurnResult(matchState, new List<int>(), hasEndedRound);
        }
        else
        {
            _logger.InfoFormat("A turn pass was rejected. PlayerId: {0}. Status: {1}.", playerId, status);
        }

        return status;
    }

    public bool SendMatchState(int playerId)
    {
        MatchState matchState = _matchRegistry.FindByPlayer(playerId);
        bool isInMatch = !matchState.IsEmpty;

        if (isInMatch)
        {
            List<int> requesterIds = new List<int> { playerId };
            NotifyUpdates(matchState, requesterIds, requesterIds);
        }
        else
        {
            _logger.InfoFormat(
                "A match state request was rejected because the player is not in a match. PlayerId: {0}.",
                playerId);
        }

        return isInMatch;
    }

    public bool LeaveMatch(int playerId)
    {
        MatchState matchState = _matchRegistry.FindByPlayer(playerId);
        bool wasRemoved = false;
        bool hasEnoughPlayers = false;

        if (!matchState.IsEmpty)
        {
            lock (matchState.SyncRoot)
            {
                wasRemoved = matchState.RemovePlayer(playerId);
                hasEnoughPlayers = matchState.HasEnoughPlayers;
            }
        }

        if (wasRemoved)
        {
            _matchRegistry.RemovePlayer(playerId);
            CompleteLeave(matchState, playerId, hasEnoughPlayers);
        }
        else
        {
            _logger.InfoFormat("A match leave was rejected because the player is not in a match. PlayerId: {0}.",
                playerId);
        }

        return wasRemoved;
    }

    private static MatchUpdatedNotice CreateMatchUpdatedNotice(
        MatchState matchState,
        IReadOnlyList<int> recipientPlayerIds)
    {
        RoundState round = matchState.CurrentRound;
        MatchUpdatedNotice notice = new MatchUpdatedNotice(
            recipientPlayerIds,
            round.CurrentPlayerId,
            round.Boneyard.Count,
            round.EngineTile,
            CountTilesByPlayer(matchState),
            CollectScoresByPlayer(matchState.PlayerIds, matchState.ScoresByPlayerId),
            CreateTrainSnapshots(round));
        return notice;
    }

    private static Dictionary<int, int> CountTilesByPlayer(MatchState matchState)
    {
        Dictionary<int, int> tileCountsByPlayerId = new Dictionary<int, int>();

        foreach (int playerId in matchState.PlayerIds)
        {
            tileCountsByPlayerId.Add(playerId, matchState.CurrentRound.GetHand(playerId).Count);
        }

        return tileCountsByPlayerId;
    }

    private static Dictionary<int, int> CollectScoresByPlayer(
        IReadOnlyList<int> playerIds,
        IReadOnlyDictionary<int, int> scoresByPlayerId)
    {
        Dictionary<int, int> collectedScoresByPlayerId = new Dictionary<int, int>();

        foreach (int playerId in playerIds)
        {
            collectedScoresByPlayerId.Add(playerId, scoresByPlayerId[playerId]);
        }

        return collectedScoresByPlayerId;
    }

    private static List<TrainSnapshot> CreateTrainSnapshots(RoundState roundState)
    {
        List<TrainSnapshot> trains = new List<TrainSnapshot>();

        foreach (Train train in roundState.Trains)
        {
            trains.Add(new TrainSnapshot(train.OwnerId, train.IsOpen, train.Tiles));
        }

        return trains;
    }

    private static RoundEndedNotice CreateRoundEndedNotice(MatchState matchState)
    {
        RoundState finishedRound = matchState.LastScoredRound;
        RoundEndedNotice notice = new RoundEndedNotice(
            new List<int>(matchState.PlayerIds),
            finishedRound.EngineTile.LeftPips,
            finishedRound.EndReason,
            CollectScoresByPlayer(matchState.PlayerIds, matchState.LastRoundScoresByPlayerId));
        return notice;
    }

    private static HandUpdatedNotice CreateHandUpdatedNotice(MatchState matchState, int playerId)
    {
        HandUpdatedNotice notice = new HandUpdatedNotice(playerId, matchState.CurrentRound.GetHand(playerId));
        return notice;
    }

    private IReadOnlyList<Tile> ShuffleTiles(DominoSet dominoSet)
    {
        IReadOnlyList<Tile> shuffledTiles = _tileShuffler.Shuffle(dominoSet.CreateTiles());
        return shuffledTiles;
    }

    private bool CloseRoundIfFinished(MatchState matchState)
    {
        bool hasEndedRound = matchState.CurrentRound.IsFinished;
        matchState.ScoreFinishedRound();

        if (matchState.IsFinished)
        {
            _matchRegistry.RemoveMatch(matchState);
            _logger.InfoFormat("A match ended. RoundEndReason: {0}.", matchState.CurrentRound.EndReason);
        }
        else if (hasEndedRound)
        {
            _logger.InfoFormat(
                "A round ended. EnginePips: {0}. RoundEndReason: {1}.",
                matchState.CurrentRound.EngineTile.LeftPips,
                matchState.CurrentRound.EndReason);
            matchState.StartNextRound(ShuffleTiles(matchState.DominoSet));
        }

        return hasEndedRound;
    }

    private void CompleteLeave(MatchState matchState, int playerId, bool hasEnoughPlayers)
    {
        NotifyUpdates(matchState, matchState.PlayerIds, new List<int>());

        if (hasEnoughPlayers)
        {
            _logger.InfoFormat("A player left the match. PlayerId: {0}.", playerId);
        }
        else
        {
            _matchRegistry.RemoveMatch(matchState);
            _logger.InfoFormat("A match was closed because too few players remain. PlayerId: {0}.", playerId);
            NotifyMatchEnded(matchState, MatchEndReason.NotEnoughPlayers);
        }
    }

    private void NotifyTurnResult(MatchState matchState, IReadOnlyList<int> turnHandOwnerIds, bool hasEndedRound)
    {
        IReadOnlyList<int> handOwnerIds = hasEndedRound ? matchState.PlayerIds : turnHandOwnerIds;
        NotifyUpdates(matchState, matchState.PlayerIds, handOwnerIds);

        if (hasEndedRound)
        {
            NotifyRoundEnded(matchState);
        }
    }

    private void NotifyRoundEnded(MatchState matchState)
    {
        RoundEndedNotice roundNotice;
        bool isMatchFinished;

        lock (matchState.SyncRoot)
        {
            roundNotice = CreateRoundEndedNotice(matchState);
            isMatchFinished = matchState.IsFinished;
        }

        _gameNotifier.NotifyRoundEnded(roundNotice);

        if (isMatchFinished)
        {
            MatchEndedNotice matchNotice = new MatchEndedNotice(
                roundNotice.RecipientPlayerIds,
                MatchEndReason.AllRoundsPlayed);
            _gameNotifier.NotifyMatchEnded(matchNotice);
        }
    }

    private void NotifyMatchEnded(MatchState matchState, MatchEndReason endReason)
    {
        MatchEndedNotice notice;

        lock (matchState.SyncRoot)
        {
            notice = new MatchEndedNotice(new List<int>(matchState.PlayerIds), endReason);
        }

        _gameNotifier.NotifyMatchEnded(notice);
    }

    private void NotifyUpdates(
        MatchState matchState,
        IReadOnlyList<int> boardRecipientIds,
        IReadOnlyList<int> handOwnerIds)
    {
        MatchUpdatedNotice matchNotice;
        List<HandUpdatedNotice> handNotices = new List<HandUpdatedNotice>();

        lock (matchState.SyncRoot)
        {
            matchNotice = CreateMatchUpdatedNotice(matchState, boardRecipientIds);

            foreach (int handOwnerId in handOwnerIds)
            {
                handNotices.Add(CreateHandUpdatedNotice(matchState, handOwnerId));
            }
        }

        _gameNotifier.NotifyMatchUpdated(matchNotice);

        foreach (HandUpdatedNotice handNotice in handNotices)
        {
            _gameNotifier.NotifyHandUpdated(handNotice);
        }
    }
}
