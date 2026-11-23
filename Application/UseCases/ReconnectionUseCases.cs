using System;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Guards;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.State;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class ReconnectionUseCases : IReconnectionUseCases
{
    private const int GracePeriodSeconds = 60;

    private static readonly ITraceLog _logger;
    private static readonly TimeSpan _gracePeriod;

    private readonly MatchRegistry _matchRegistry;
    private readonly IMatchUseCases _matchUseCases;
    private readonly IGracePeriodScheduler _gracePeriodScheduler;

    static ReconnectionUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(ReconnectionUseCases));
        _gracePeriod = TimeSpan.FromSeconds(GracePeriodSeconds);
    }

    public ReconnectionUseCases(
        MatchRegistry matchRegistry,
        IMatchUseCases matchUseCases,
        IGracePeriodScheduler gracePeriodScheduler)
    {
        _matchRegistry = ArgumentGuard.NotNull(matchRegistry, nameof(matchRegistry), _logger);
        _matchUseCases = ArgumentGuard.NotNull(matchUseCases, nameof(matchUseCases), _logger);
        _gracePeriodScheduler = ArgumentGuard.NotNull(gracePeriodScheduler, nameof(gracePeriodScheduler), _logger);
    }

    public bool HoldSeat(int playerId)
    {
        MatchState matchState = _matchRegistry.FindByPlayer(playerId);
        bool isSeatHeld = false;

        if (!matchState.IsEmpty)
        {
            lock (matchState.SyncRoot)
            {
                isSeatHeld = !matchState.IsFinished;
            }
        }

        if (isSeatHeld)
        {
            _gracePeriodScheduler.Start(playerId, _gracePeriod, ExpireGracePeriod);
            _logger.InfoFormat(
                "A disconnected player keeps the match seat during the grace period. PlayerId: {0}. Seconds: {1}.",
                playerId,
                GracePeriodSeconds);
        }

        return isSeatHeld;
    }

    public bool Reconnect(int playerId, string matchCode)
    {
        ArgumentGuard.ThrowIfEmpty(matchCode, nameof(matchCode), _logger);

        MatchState matchState = _matchRegistry.FindByPlayer(playerId);
        bool isSameMatch = string.Equals(matchState.Code, matchCode, StringComparison.Ordinal);
        bool wasSeatHeld = isSameMatch && _gracePeriodScheduler.Cancel(playerId);
        bool wasReconnected = false;

        if (wasSeatHeld)
        {
            wasReconnected = _matchUseCases.SendMatchState(playerId);
        }

        if (wasReconnected)
        {
            _logger.InfoFormat("A player reconnected to the match. PlayerId: {0}.", playerId);
        }
        else
        {
            _logger.InfoFormat(
                "A match reconnection was rejected because the player has no held seat. PlayerId: {0}.",
                playerId);
        }

        return wasReconnected;
    }

    private void ExpireGracePeriod(int playerId)
    {
        _logger.InfoFormat("The reconnection grace period expired. PlayerId: {0}.", playerId);
        _matchUseCases.LeaveMatch(playerId);
    }
}
