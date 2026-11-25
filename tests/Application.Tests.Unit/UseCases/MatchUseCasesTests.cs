using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.State;
using DominoTrainGame.Server.Application.UseCases;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace DominoTrainGame.Server.Application.Tests.Unit.UseCases;

[TestFixture]
public sealed class MatchUseCasesTests
{
    private const int FirstPlayerId = 7;
    private const int SecondPlayerId = 12;
    private const int ThirdPlayerId = 21;
    private const int PlayerWithoutMatchId = 40;
    private const int ExpectedCallCount = 1;
    private const int FirstArgumentIndex = 0;
    private const string MatchCode = "ABC123";
    private const string DatabaseErrorMessage = "The database is unavailable.";
    private const string DataIntegrityErrorMessage = "The match results violate the database rules.";

    private MatchRegistry _matchRegistry;
    private ITileShuffler _tileShuffler;
    private IGameNotifier _gameNotifier;
    private IMatchResultRepository _matchResultRepository;
    private MatchUseCases _matchUseCases;

    [SetUp]
    public void SetUp()
    {
        _matchRegistry = new MatchRegistry();
        _tileShuffler = Substitute.For<ITileShuffler>();
        _gameNotifier = Substitute.For<IGameNotifier>();
        _matchResultRepository = Substitute.For<IMatchResultRepository>();

        _tileShuffler.Shuffle(Arg.Any<IReadOnlyList<Tile>>()).Returns(DominoSet.DoubleSix.CreateTiles());
        _matchResultRepository.SaveMatchResults(Arg.Any<FinishedMatchRecord>()).Returns(true);

        CreateMatchUseCases();
        _matchUseCases.StartMatch(
            MatchCode,
            new List<int> { FirstPlayerId, SecondPlayerId, ThirdPlayerId },
            DominoSet.DoubleSix);
        _gameNotifier.ClearReceivedCalls();
    }

    [Test]
    public void Test_Constructor_NullMatchRegistry_ThrowsArgumentNullException()
    {
        _matchRegistry = null;

        Assert.Throws<ArgumentNullException>(CreateMatchUseCases);
    }

    [Test]
    public void Test_Constructor_NullTileShuffler_ThrowsArgumentNullException()
    {
        _tileShuffler = null;

        Assert.Throws<ArgumentNullException>(CreateMatchUseCases);
    }

    [Test]
    public void Test_Constructor_NullGameNotifier_ThrowsArgumentNullException()
    {
        _gameNotifier = null;

        Assert.Throws<ArgumentNullException>(CreateMatchUseCases);
    }

    [Test]
    public void Test_Constructor_NullMatchResultRepository_ThrowsArgumentNullException()
    {
        _matchResultRepository = null;

        Assert.Throws<ArgumentNullException>(CreateMatchUseCases);
    }

    [Test]
    public void Test_SendMatchState_PlayerInMatch_ReturnsTrue()
    {
        bool isInMatch = _matchUseCases.SendMatchState(FirstPlayerId);

        Assert.That(isInMatch, Is.True);
    }

    [Test]
    public void Test_SendMatchState_PlayerInMatch_SendsBoardOnlyToRequester()
    {
        _matchUseCases.SendMatchState(FirstPlayerId);

        MatchUpdatedNotice boardNotice = FindNotices<MatchUpdatedNotice>().Single();
        Assert.That(boardNotice.RecipientPlayerIds, Is.EqualTo(new[] { FirstPlayerId }));
    }

    [Test]
    public void Test_SendMatchState_PlayerInMatch_SendsOnlyRequesterHand()
    {
        _matchUseCases.SendMatchState(FirstPlayerId);

        HandUpdatedNotice handNotice = FindNotices<HandUpdatedNotice>().Single();
        Assert.That(handNotice.RecipientPlayerId, Is.EqualTo(FirstPlayerId));
    }

    [Test]
    public void Test_SendMatchState_PlayerWithoutMatch_ReturnsFalse()
    {
        bool isInMatch = _matchUseCases.SendMatchState(PlayerWithoutMatchId);

        Assert.That(isInMatch, Is.False);
    }

    [Test]
    public void Test_SendMatchState_PlayerWithoutMatch_DoesNotNotifyBoard()
    {
        _matchUseCases.SendMatchState(PlayerWithoutMatchId);

        _gameNotifier.DidNotReceive().NotifyMatchUpdated(Arg.Any<MatchUpdatedNotice>());
    }

    [Test]
    public void Test_LeaveMatch_PlayerInMatch_ReturnsTrue()
    {
        bool wasRemoved = _matchUseCases.LeaveMatch(ThirdPlayerId);

        Assert.That(wasRemoved, Is.True);
    }

    [Test]
    public void Test_LeaveMatch_PlayerInMatch_RemovesPlayerFromRegistry()
    {
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        MatchState matchState = _matchRegistry.FindByPlayer(ThirdPlayerId);
        Assert.That(matchState.IsEmpty, Is.True);
    }

    [Test]
    public void Test_LeaveMatch_EnoughPlayersRemain_KeepsMatchForRemainingPlayers()
    {
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        MatchState matchState = _matchRegistry.FindByPlayer(FirstPlayerId);
        Assert.That(matchState.IsEmpty, Is.False);
    }

    [Test]
    public void Test_LeaveMatch_EnoughPlayersRemain_SendsBoardToRemainingPlayers()
    {
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        MatchUpdatedNotice boardNotice = FindNotices<MatchUpdatedNotice>().Single();
        Assert.That(boardNotice.RecipientPlayerIds, Is.EqualTo(new[] { FirstPlayerId, SecondPlayerId }));
    }

    [Test]
    public void Test_LeaveMatch_EnoughPlayersRemain_DoesNotEndMatch()
    {
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        _gameNotifier.DidNotReceive().NotifyMatchEnded(Arg.Any<MatchEndedNotice>());
    }

    [Test]
    public void Test_LeaveMatch_EnoughPlayersRemain_DoesNotSaveMatchResults()
    {
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        _matchResultRepository.DidNotReceive().SaveMatchResults(Arg.Any<FinishedMatchRecord>());
    }

    [Test]
    public void Test_LeaveMatch_PlayerWithoutMatch_ReturnsFalse()
    {
        bool wasRemoved = _matchUseCases.LeaveMatch(PlayerWithoutMatchId);

        Assert.That(wasRemoved, Is.False);
    }

    [Test]
    public void Test_LeaveMatch_PlayerWithoutMatch_DoesNotNotifyBoard()
    {
        _matchUseCases.LeaveMatch(PlayerWithoutMatchId);

        _gameNotifier.DidNotReceive().NotifyMatchUpdated(Arg.Any<MatchUpdatedNotice>());
    }

    [Test]
    public void Test_LeaveMatch_PlayerAlreadyLeft_ReturnsFalse()
    {
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        bool wasRemoved = _matchUseCases.LeaveMatch(ThirdPlayerId);

        Assert.That(wasRemoved, Is.False);
    }

    [Test]
    public void Test_LeaveMatch_TooFewPlayersRemain_EndsMatchForNotEnoughPlayers()
    {
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        _matchUseCases.LeaveMatch(SecondPlayerId);

        MatchEndedNotice endNotice = FindNotices<MatchEndedNotice>().Single();
        Assert.That(endNotice.EndReason, Is.EqualTo(MatchEndReason.NotEnoughPlayers));
    }

    [Test]
    public void Test_LeaveMatch_TooFewPlayersRemain_NotifiesRemainingPlayer()
    {
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        _matchUseCases.LeaveMatch(SecondPlayerId);

        MatchEndedNotice endNotice = FindNotices<MatchEndedNotice>().Single();
        Assert.That(endNotice.RecipientPlayerIds, Is.EqualTo(new[] { FirstPlayerId }));
    }

    [Test]
    public void Test_LeaveMatch_TooFewPlayersRemain_RemovesMatchFromRegistry()
    {
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        _matchUseCases.LeaveMatch(SecondPlayerId);

        MatchState matchState = _matchRegistry.FindByPlayer(FirstPlayerId);
        Assert.That(matchState.IsEmpty, Is.True);
    }

    [Test]
    public void Test_LeaveMatch_TooFewPlayersRemain_SavesMatchResultsOnce()
    {
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        _matchUseCases.LeaveMatch(SecondPlayerId);

        _matchResultRepository.Received(ExpectedCallCount).SaveMatchResults(Arg.Any<FinishedMatchRecord>());
    }

    [Test]
    public void Test_LeaveMatch_TooFewPlayersRemain_SavesResultsWithMatchCode()
    {
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        _matchUseCases.LeaveMatch(SecondPlayerId);

        FinishedMatchRecord savedMatch = FindSavedMatch();
        Assert.That(savedMatch.LobbyCode, Is.EqualTo(MatchCode));
    }

    [Test]
    public void Test_LeaveMatch_ResultsNotSaved_ReturnsTrue()
    {
        _matchResultRepository.SaveMatchResults(Arg.Any<FinishedMatchRecord>()).Returns(false);
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        bool wasRemoved = _matchUseCases.LeaveMatch(SecondPlayerId);

        Assert.That(wasRemoved, Is.True);
    }

    [Test]
    public void Test_LeaveMatch_DatabaseUnavailableWhileSavingResults_DoesNotThrow()
    {
        _matchResultRepository.SaveMatchResults(Arg.Any<FinishedMatchRecord>())
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        Assert.DoesNotThrow(LeaveMatchAsSecondPlayer);
    }

    [Test]
    public void Test_LeaveMatch_DatabaseUnavailableWhileSavingResults_StillEndsMatch()
    {
        _matchResultRepository.SaveMatchResults(Arg.Any<FinishedMatchRecord>())
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        _matchUseCases.LeaveMatch(SecondPlayerId);

        _gameNotifier.Received(ExpectedCallCount).NotifyMatchEnded(Arg.Any<MatchEndedNotice>());
    }

    [Test]
    public void Test_LeaveMatch_ResultsViolateDatabaseRules_DoesNotThrow()
    {
        _matchResultRepository.SaveMatchResults(Arg.Any<FinishedMatchRecord>())
            .Throws(new DataIntegrityException(DataIntegrityErrorMessage));
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        Assert.DoesNotThrow(LeaveMatchAsSecondPlayer);
    }

    [Test]
    public void Test_LeaveMatch_ResultsViolateDatabaseRules_StillEndsMatch()
    {
        _matchResultRepository.SaveMatchResults(Arg.Any<FinishedMatchRecord>())
            .Throws(new DataIntegrityException(DataIntegrityErrorMessage));
        _matchUseCases.LeaveMatch(ThirdPlayerId);

        _matchUseCases.LeaveMatch(SecondPlayerId);

        _gameNotifier.Received(ExpectedCallCount).NotifyMatchEnded(Arg.Any<MatchEndedNotice>());
    }

    private void CreateMatchUseCases()
    {
        _matchUseCases = new MatchUseCases(_matchRegistry, _tileShuffler, _gameNotifier, _matchResultRepository);
    }

    private void LeaveMatchAsSecondPlayer()
    {
        _matchUseCases.LeaveMatch(SecondPlayerId);
    }

    private List<TNotice> FindNotices<TNotice>() where TNotice : class
    {
        List<TNotice> notices = new List<TNotice>();

        foreach (ICall notifierCall in _gameNotifier.ReceivedCalls())
        {
            if (notifierCall.GetArguments()[FirstArgumentIndex] is TNotice notice)
            {
                notices.Add(notice);
            }
        }

        return notices;
    }

    private FinishedMatchRecord FindSavedMatch()
    {
        ICall saveCall = _matchResultRepository.ReceivedCalls().Single();
        FinishedMatchRecord finishedMatch = (FinishedMatchRecord)saveCall.GetArguments()[FirstArgumentIndex];
        return finishedMatch;
    }
}
