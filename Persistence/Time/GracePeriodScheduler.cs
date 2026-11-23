using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using DominoTrainGame.Server.Application.Guards;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Persistence.Time;

public sealed class GracePeriodScheduler : IGracePeriodScheduler
{
    private static readonly ILog _logger;

    private readonly Dictionary<int, GracePeriod> _gracePeriodsByPlayerId = new Dictionary<int, GracePeriod>();
    private readonly object _gracePeriodsLock = new object();

    static GracePeriodScheduler()
    {
        _logger = LogManager.GetLogger(typeof(GracePeriodScheduler));
    }

    public void Start(int playerId, TimeSpan gracePeriod, Action<int> expire)
    {
        ArgumentGuard.ThrowIfFull(expire, nameof(expire), _logger);
        GracePeriod newGracePeriod = new GracePeriod(this, playerId, expire);

        lock (_gracePeriodsLock)
        {
            StopGracePeriod(playerId);
            _gracePeriodsByPlayerId.Add(playerId, newGracePeriod);
            newGracePeriod.Start(gracePeriod);
        }
    }

    public bool Cancel(int playerId)
    {
        bool wasCanceled;

        lock (_gracePeriodsLock)
        {
            wasCanceled = StopGracePeriod(playerId);
        }

        return wasCanceled;
    }

    private bool StopGracePeriod(int playerId)
    {
        bool wasPending = _gracePeriodsByPlayerId.ContainsKey(playerId);

        if (wasPending)
        {
            _gracePeriodsByPlayerId[playerId].Dispose();
            _gracePeriodsByPlayerId.Remove(playerId);
        }

        return wasPending;
    }

    private void Expire(GracePeriod gracePeriod)
    {
        int playerId = gracePeriod.PlayerId;
        bool wasPending;

        lock (_gracePeriodsLock)
        {
            wasPending = _gracePeriodsByPlayerId.ContainsKey(playerId)
                && ReferenceEquals(_gracePeriodsByPlayerId[playerId], gracePeriod);

            if (wasPending)
            {
                _gracePeriodsByPlayerId.Remove(playerId);
            }
        }

        gracePeriod.Dispose();

        if (wasPending)
        {
            gracePeriod.Notify();
        }
    }

    private sealed class GracePeriod : IDisposable
    {
        private readonly GracePeriodScheduler _scheduler;
        private readonly Action<int> _expire;
        private readonly Timer _timer;

        public GracePeriod(GracePeriodScheduler scheduler, int playerId, Action<int> expire)
        {
            _scheduler = scheduler;
            _expire = expire;
            PlayerId = playerId;
            _timer = new Timer(HandleTimerElapsed);
        }

        public int PlayerId { get; }

        public void Start(TimeSpan gracePeriod)
        {
            _timer.Change(gracePeriod, Timeout.InfiniteTimeSpan);
        }

        public void Notify()
        {
            _expire(PlayerId);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private void HandleTimerElapsed(object state)
        {
            _scheduler.Expire(this);
        }
    }
}
