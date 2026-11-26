using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.State;
using DominoTrainGame.Server.Application.UseCases;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;

namespace DominoTrainGame.Server.Application.Tests.Unit.UseCases;

[TestFixture]
public sealed class ReconnectionUseCasesTests
{
    private const int DisconnectedPlayerId = 7;
    private const int OpponentPlayerId = 12;
    private const int PlayerWithoutMatchId = 40;
    private const int ExpectedCallCount = 1;
    private const int GracePeriodSeconds = 60;
    private const int ExpireArgumentIndex = 2;
    private const int DealtTileCount = 14;
    private const int FirstPassNumber = 0;
    private const string MatchCode = "ABC123";
    private const string OtherMatchCode = "XYZ789";
    private const string EmptyText = "";
    private const string WhitespaceMatchCode = "   ";

    private static readonly TimeSpan _gracePeriod = TimeSpan.FromSeconds(GracePeriodSeconds);

    private MatchRegistry _matchRegistry;
    private IMatchUseCases _matchUseCases;
    private IGracePeriodScheduler _gracePeriodScheduler;
    private ReconnectionUseCases _reconnectionUseCases;
    private MatchState _matchState;
    private string _matchCode;

    [SetUp]
    public void SetUp()
    {
        _matchRegistry = new MatchRegistry();
        _matchUseCases = Substitute.For<IMatchUseCases>();
        _gracePeriodScheduler = Substitute.For<IGracePeriodScheduler>();
        _matchState = new MatchState(
            MatchCode,
            new List<int> { DisconnectedPlayerId, OpponentPlayerId },
            DominoSet.DoubleSix,
            CreateBlockedRoundTiles(DominoSet.DoubleSix.HighestDouble));

        _matchRegistry.TryAdd(_matchState);
        _gracePeriodScheduler.Cancel(DisconnectedPlayerId).Returns(true);
        _matchUseCases.SendMatchState(DisconnectedPlayerId).Returns(true);

        _matchCode = MatchCode;
        CreateReconnectionUseCases();
    }

    [Test]
    public void Test_Constructor_NullMatchRegistry_ThrowsArgumentNullException()
    {
        _matchRegistry = null;

        Assert.Throws<ArgumentNullException>(CreateReconnectionUseCases);
    }

    [Test]
    public void Test_Constructor_NullMatchUseCases_ThrowsArgumentNullException()
    {
        _matchUseCases = null;

        Assert.Throws<ArgumentNullException>(CreateReconnectionUseCases);
    }

    [Test]
    public void Test_Constructor_NullGracePeriodScheduler_ThrowsArgumentNullException()
    {
        _gracePeriodScheduler = null;

        Assert.Throws<ArgumentNullException>(CreateReconnectionUseCases);
    }

    [Test]
    public void Test_HoldSeat_PlayerInActiveMatch_ReturnsTrue()
    {
        bool isSeatHeld = _reconnectionUseCases.HoldSeat(DisconnectedPlayerId);

        Assert.That(isSeatHeld, Is.True);
    }

    [Test]
    public void Test_HoldSeat_PlayerInActiveMatch_StartsGracePeriodOnce()
    {
        _reconnectionUseCases.HoldSeat(DisconnectedPlayerId);

        _gracePeriodScheduler.Received(ExpectedCallCount)
            .Start(DisconnectedPlayerId, _gracePeriod, Arg.Any<Action<int>>());
    }

    [Test]
    public void Test_HoldSeat_PlayerInActiveMatch_DoesNotLeaveMatchBeforeExpiration()
    {
        _reconnectionUseCases.HoldSeat(DisconnectedPlayerId);

        _matchUseCases.DidNotReceive().LeaveMatch(Arg.Any<int>());
    }

    [Test]
    public void Test_HoldSeat_GracePeriodExpires_LeavesMatchOnce()
    {
        _reconnectionUseCases.HoldSeat(DisconnectedPlayerId);
        Action<int> expireGracePeriod = FindExpireAction();

        expireGracePeriod(DisconnectedPlayerId);

        _matchUseCases.Received(ExpectedCallCount).LeaveMatch(DisconnectedPlayerId);
    }

    [Test]
    public void Test_HoldSeat_PlayerWithoutMatch_ReturnsFalse()
    {
        bool isSeatHeld = _reconnectionUseCases.HoldSeat(PlayerWithoutMatchId);

        Assert.That(isSeatHeld, Is.False);
    }

    [Test]
    public void Test_HoldSeat_PlayerWithoutMatch_DoesNotStartGracePeriod()
    {
        _reconnectionUseCases.HoldSeat(PlayerWithoutMatchId);

        _gracePeriodScheduler.DidNotReceive().Start(Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<Action<int>>());
    }

    [Test]
    public void Test_HoldSeat_FinishedMatch_ReturnsFalse()
    {
        FinishAllRounds();

        bool isSeatHeld = _reconnectionUseCases.HoldSeat(DisconnectedPlayerId);

        Assert.That(isSeatHeld, Is.False);
    }

    [Test]
    public void Test_HoldSeat_FinishedMatch_DoesNotStartGracePeriod()
    {
        FinishAllRounds();

        _reconnectionUseCases.HoldSeat(DisconnectedPlayerId);

        _gracePeriodScheduler.DidNotReceive().Start(Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<Action<int>>());
    }

    [Test]
    public void Test_Reconnect_HeldSeatInSameMatch_ReturnsTrue()
    {
        bool wasReconnected = _reconnectionUseCases.Reconnect(DisconnectedPlayerId, _matchCode);

        Assert.That(wasReconnected, Is.True);
    }

    [Test]
    public void Test_Reconnect_HeldSeatInSameMatch_CancelsGracePeriodOnce()
    {
        _reconnectionUseCases.Reconnect(DisconnectedPlayerId, _matchCode);

        _gracePeriodScheduler.Received(ExpectedCallCount).Cancel(DisconnectedPlayerId);
    }

    [Test]
    public void Test_Reconnect_HeldSeatInSameMatch_SendsMatchStateOnce()
    {
        _reconnectionUseCases.Reconnect(DisconnectedPlayerId, _matchCode);

        _matchUseCases.Received(ExpectedCallCount).SendMatchState(DisconnectedPlayerId);
    }

    [Test]
    public void Test_Reconnect_OtherMatchCode_ReturnsFalse()
    {
        _matchCode = OtherMatchCode;

        bool wasReconnected = _reconnectionUseCases.Reconnect(DisconnectedPlayerId, _matchCode);

        Assert.That(wasReconnected, Is.False);
    }

    [Test]
    public void Test_Reconnect_OtherMatchCode_KeepsGracePeriodRunning()
    {
        _matchCode = OtherMatchCode;

        _reconnectionUseCases.Reconnect(DisconnectedPlayerId, _matchCode);

        _gracePeriodScheduler.DidNotReceive().Cancel(Arg.Any<int>());
    }

    [Test]
    public void Test_Reconnect_PlayerWithoutMatch_ReturnsFalse()
    {
        bool wasReconnected = _reconnectionUseCases.Reconnect(PlayerWithoutMatchId, _matchCode);

        Assert.That(wasReconnected, Is.False);
    }

    [Test]
    public void Test_Reconnect_NoHeldSeat_ReturnsFalse()
    {
        _gracePeriodScheduler.Cancel(DisconnectedPlayerId).Returns(false);

        bool wasReconnected = _reconnectionUseCases.Reconnect(DisconnectedPlayerId, _matchCode);

        Assert.That(wasReconnected, Is.False);
    }

    [Test]
    public void Test_Reconnect_NoHeldSeat_DoesNotSendMatchState()
    {
        _gracePeriodScheduler.Cancel(DisconnectedPlayerId).Returns(false);

        _reconnectionUseCases.Reconnect(DisconnectedPlayerId, _matchCode);

        _matchUseCases.DidNotReceive().SendMatchState(Arg.Any<int>());
    }

    [Test]
    public void Test_Reconnect_MatchStateNotSent_ReturnsFalse()
    {
        _matchUseCases.SendMatchState(DisconnectedPlayerId).Returns(false);

        bool wasReconnected = _reconnectionUseCases.Reconnect(DisconnectedPlayerId, _matchCode);

        Assert.That(wasReconnected, Is.False);
    }

    [TestCase(EmptyText)]
    [TestCase(WhitespaceMatchCode)]
    [TestCase(null)]
    public void Test_Reconnect_BlankMatchCode_ThrowsArgumentException(string blankMatchCode)
    {
        _matchCode = blankMatchCode;

        Assert.Throws<ArgumentException>(ReconnectWithArrangedCode);
    }

    private static List<Tile> CreateBlockedRoundTiles(int enginePips)
    {
        Tile engineTile = new Tile(enginePips, enginePips);
        List<Tile> roundTiles = new List<Tile> { engineTile };

        foreach (Tile tile in DominoSet.DoubleSix.CreateTiles())
        {
            bool canDealTile = !tile.IsSameTile(engineTile) && roundTiles.Count <= DealtTileCount;

            if (canDealTile)
            {
                roundTiles.Add(tile);
            }
        }

        return roundTiles;
    }

    private void CreateReconnectionUseCases()
    {
        _reconnectionUseCases = new ReconnectionUseCases(_matchRegistry, _matchUseCases, _gracePeriodScheduler);
    }

    private void ReconnectWithArrangedCode()
    {
        _reconnectionUseCases.Reconnect(DisconnectedPlayerId, _matchCode);
    }

    private Action<int> FindExpireAction()
    {
        ICall startCall = _gracePeriodScheduler.ReceivedCalls().Single();
        Action<int> expireGracePeriod = (Action<int>)startCall.GetArguments()[ExpireArgumentIndex];
        return expireGracePeriod;
    }

    private void FinishAllRounds()
    {
        BlockCurrentRound();

        while (_matchState.HasNextRound)
        {
            int nextEnginePips = _matchState.CurrentRound.EngineTile.LeftPips;
            nextEnginePips--;
            _matchState.StartNextRound(CreateBlockedRoundTiles(nextEnginePips));
            BlockCurrentRound();
        }
    }

    private void BlockCurrentRound()
    {
        RoundState round = _matchState.CurrentRound;

        for (int passNumber = FirstPassNumber; passNumber < _matchState.PlayerIds.Count; passNumber++)
        {
            round.PassTurn(round.CurrentPlayerId);
        }
    }
}
