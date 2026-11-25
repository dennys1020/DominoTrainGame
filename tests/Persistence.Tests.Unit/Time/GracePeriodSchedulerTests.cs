using System;
using System.Threading;
using DominoTrainGame.Server.Persistence.Time;
using NUnit.Framework;

namespace DominoTrainGame.Server.Persistence.Tests.Unit.Time;

[TestFixture]
public sealed class GracePeriodSchedulerTests
{
    private const int DisconnectedPlayerId = 7;
    private const int OtherPlayerId = 12;
    private const int NoExpiredPlayerId = 0;
    private const int PendingGracePeriodMinutes = 10;
    private const int ExpirationTimeoutSeconds = 5;

    private static readonly TimeSpan _pendingGracePeriod = TimeSpan.FromMinutes(PendingGracePeriodMinutes);
    private static readonly TimeSpan _elapsedGracePeriod = TimeSpan.Zero;
    private static readonly TimeSpan _expirationTimeout = TimeSpan.FromSeconds(ExpirationTimeoutSeconds);

    private GracePeriodScheduler _gracePeriodScheduler;
    private ManualResetEventSlim _expirationSignal;
    private Action<int> _expire;
    private int _expiredPlayerId;

    [SetUp]
    public void SetUp()
    {
        _gracePeriodScheduler = new GracePeriodScheduler();
        _expirationSignal = new ManualResetEventSlim(false);
        _expire = RecordExpiration;
        _expiredPlayerId = NoExpiredPlayerId;
    }

    [TearDown]
    public void TearDown()
    {
        _gracePeriodScheduler.Cancel(DisconnectedPlayerId);
        _expirationSignal.Dispose();
    }

    [Test]
    public void Test_Start_NullExpireAction_ThrowsArgumentNullException()
    {
        _expire = null;

        Assert.Throws<ArgumentNullException>(StartPendingGracePeriod);
    }

    [Test]
    public void Test_Cancel_PendingGracePeriod_ReturnsTrue()
    {
        StartPendingGracePeriod();

        bool wasCanceled = _gracePeriodScheduler.Cancel(DisconnectedPlayerId);

        Assert.That(wasCanceled, Is.True);
    }

    [Test]
    public void Test_Cancel_NoGracePeriod_ReturnsFalse()
    {
        bool wasCanceled = _gracePeriodScheduler.Cancel(DisconnectedPlayerId);

        Assert.That(wasCanceled, Is.False);
    }

    [Test]
    public void Test_Cancel_AlreadyCanceledGracePeriod_ReturnsFalse()
    {
        StartPendingGracePeriod();
        _gracePeriodScheduler.Cancel(DisconnectedPlayerId);

        bool wasCanceled = _gracePeriodScheduler.Cancel(DisconnectedPlayerId);

        Assert.That(wasCanceled, Is.False);
    }

    [Test]
    public void Test_Cancel_OtherPlayerHasGracePeriod_ReturnsFalse()
    {
        StartPendingGracePeriod();

        bool wasCanceled = _gracePeriodScheduler.Cancel(OtherPlayerId);

        Assert.That(wasCanceled, Is.False);
    }

    [Test]
    public void Test_Cancel_RestartedGracePeriod_ReturnsTrue()
    {
        StartPendingGracePeriod();
        StartPendingGracePeriod();

        bool wasCanceled = _gracePeriodScheduler.Cancel(DisconnectedPlayerId);

        Assert.That(wasCanceled, Is.True);
    }

    [Test]
    public void Test_Start_GracePeriodElapses_CallsExpireAction()
    {
        _gracePeriodScheduler.Start(DisconnectedPlayerId, _elapsedGracePeriod, _expire);

        bool hasExpired = _expirationSignal.Wait(_expirationTimeout);

        Assert.That(hasExpired, Is.True);
    }

    [Test]
    public void Test_Start_GracePeriodElapses_PassesPlayerIdToExpireAction()
    {
        _gracePeriodScheduler.Start(DisconnectedPlayerId, _elapsedGracePeriod, _expire);

        _expirationSignal.Wait(_expirationTimeout);

        Assert.That(_expiredPlayerId, Is.EqualTo(DisconnectedPlayerId));
    }

    [Test]
    public void Test_Cancel_ExpiredGracePeriod_ReturnsFalse()
    {
        _gracePeriodScheduler.Start(DisconnectedPlayerId, _elapsedGracePeriod, _expire);
        _expirationSignal.Wait(_expirationTimeout);

        bool wasCanceled = _gracePeriodScheduler.Cancel(DisconnectedPlayerId);

        Assert.That(wasCanceled, Is.False);
    }

    private void StartPendingGracePeriod()
    {
        _gracePeriodScheduler.Start(DisconnectedPlayerId, _pendingGracePeriod, _expire);
    }

    private void RecordExpiration(int playerId)
    {
        _expiredPlayerId = playerId;
        _expirationSignal.Set();
    }
}
