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
        MatchState match = new MatchState(playerIds, ShuffleTiles());
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
        bool hasEndedRound = false;

        if (!match.IsEmpty)
        {
            lock (match.SyncRoot)
            {
                status = match.IsFinished
                    ? PlayTileStatus.MatchNotFound
                    : match.CurrentRound.PlayTile(playerId, tile, trainOwnerId);
                hasEndedRound = status == PlayTileStatus.Success && CloseRoundIfFinished(match);
            }
        }

        if (status == PlayTileStatus.Success)
        {
            NotifyTurnResult(match, new List<int> { playerId }, hasEndedRound);
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
                status = match.IsFinished
                    ? DrawTileStatus.MatchNotFound
                    : match.CurrentRound.DrawTile(playerId);
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
        bool hasEndedRound = false;

        if (!match.IsEmpty)
        {
            lock (match.SyncRoot)
            {
                status = match.IsFinished
                    ? PassTurnStatus.MatchNotFound
                    : match.CurrentRound.PassTurn(playerId);
                hasEndedRound = status == PassTurnStatus.Success && CloseRoundIfFinished(match);
            }
        }

        if (status == PassTurnStatus.Success)
        {
            NotifyTurnResult(match, new List<int>(), hasEndedRound);
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

    private static MatchUpdatedNotice CreateMatchUpdatedNotice(MatchState match, IReadOnlyList<int> recipientPlayerIds)
    {
        RoundState round = match.CurrentRound;
        MatchUpdatedNotice notice = new MatchUpdatedNotice(
            recipientPlayerIds,
            round.CurrentPlayerId,
            round.Boneyard.Count,
            round.EngineTile,
            CountTilesByPlayer(match),
            CollectScoresByPlayer(match.PlayerIds, match.ScoresByPlayerId),
            CreateTrainSnapshots(round));
        return notice;
    }

    private static Dictionary<int, int> CountTilesByPlayer(MatchState match)
    {
        Dictionary<int, int> tileCountsByPlayerId = new Dictionary<int, int>();

        foreach (int playerId in match.PlayerIds)
        {
            tileCountsByPlayerId.Add(playerId, match.CurrentRound.GetHand(playerId).Count);
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

    private static List<TrainSnapshot> CreateTrainSnapshots(RoundState round)
    {
        List<TrainSnapshot> trains = new List<TrainSnapshot>();

        foreach (Train train in round.Trains)
        {
            trains.Add(new TrainSnapshot(train.OwnerId, train.IsOpen, train.Tiles));
        }

        return trains;
    }

    private static RoundEndedNotice CreateRoundEndedNotice(MatchState match)
    {
        RoundState finishedRound = match.LastScoredRound;
        RoundEndedNotice notice = new RoundEndedNotice(
            new List<int>(match.PlayerIds),
            finishedRound.EngineTile.LeftPips,
            finishedRound.EndReason,
            CollectScoresByPlayer(match.PlayerIds, match.LastRoundScoresByPlayerId));
        return notice;
    }

    private static HandUpdatedNotice CreateHandUpdatedNotice(MatchState match, int playerId)
    {
        HandUpdatedNotice notice = new HandUpdatedNotice(playerId, match.CurrentRound.GetHand(playerId));
        return notice;
    }

    private IReadOnlyList<Tile> ShuffleTiles()
    {
        IReadOnlyList<Tile> shuffledTiles = _tileShuffler.Shuffle(Tile.CreateDoubleTwelveSet());
        return shuffledTiles;
    }

    private bool CloseRoundIfFinished(MatchState match)
    {
        bool hasEndedRound = match.CurrentRound.IsFinished;
        match.ScoreFinishedRound();

        if (match.IsFinished)
        {
            _matchRegistry.RemoveMatch(match);
            _logger.InfoFormat("A match ended. RoundEndReason: {0}.", match.CurrentRound.EndReason);
        }
        else if (hasEndedRound)
        {
            _logger.InfoFormat(
                "A round ended. EnginePips: {0}. RoundEndReason: {1}.",
                match.CurrentRound.EngineTile.LeftPips,
                match.CurrentRound.EndReason);
            match.StartNextRound(ShuffleTiles());
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

    private void NotifyTurnResult(MatchState match, IReadOnlyList<int> turnHandOwnerIds, bool hasEndedRound)
    {
        IReadOnlyList<int> handOwnerIds = hasEndedRound ? match.PlayerIds : turnHandOwnerIds;
        NotifyUpdates(match, match.PlayerIds, handOwnerIds);

        if (hasEndedRound)
        {
            NotifyRoundEnded(match);
        }
    }

    private void NotifyRoundEnded(MatchState match)
    {
        RoundEndedNotice roundNotice;
        bool isMatchFinished;

        lock (match.SyncRoot)
        {
            roundNotice = CreateRoundEndedNotice(match);
            isMatchFinished = match.IsFinished;
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

    private void NotifyMatchEnded(MatchState match, MatchEndReason endReason)
    {
        MatchEndedNotice notice;

        lock (match.SyncRoot)
        {
            notice = new MatchEndedNotice(new List<int>(match.PlayerIds), endReason);
        }

        _gameNotifier.NotifyMatchEnded(notice);
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
