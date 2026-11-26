using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Persistence.Data;
using DominoTrainGame.Server.Persistence.Data.Repositories;
using DominoTrainGame.Server.Persistence.Time;
using NUnit.Framework;

namespace DominoTrainGame.Server.Persistence.Tests.Integration.Data.Repositories;

[TestFixture]
public sealed class MatchResultRepositoryTests
{
    private const int WinnerScore = 15;
    private const int LoserScore = 48;
    private const int GuestScore = 63;
    private const int WinnerPointsAfterTwoMatches = WinnerScore * ExpectedMatchCount;
    private const int ExpectedMatchCount = 2;
    private const int SingleCount = 1;
    private const int LeaderboardLimit = 10;
    private const int UnknownPlayerId = -1;
    private const string LobbyCode = "ABC123";
    private const string WinnerUsername = "samuel";
    private const string WinnerEmail = "samuel@uv.mx";
    private const string LoserUsername = "denisse";
    private const string LoserEmail = "denisse@uv.mx";
    private const string GuestUsername = "Guest123456";
    private const string PasswordHash = "stored-hash";
    private const string UsernamePropertyName = nameof(PlayerStatistics.Username);

    private TransactionScope _transaction;
    private DatabaseExecutor _databaseExecutor;
    private PlayerRepository _playerRepository;
    private MatchResultRepository _matchResultRepository;
    private FinishedMatchRecord _finishedMatch;
    private int _winnerPlayerId;
    private int _loserPlayerId;
    private int _guestPlayerId;

    [SetUp]
    public void SetUp()
    {
        _transaction = new TransactionScope();
        _databaseExecutor = new DatabaseExecutor();
        _playerRepository = new PlayerRepository(_databaseExecutor, new SystemClock());
        _matchResultRepository = new MatchResultRepository(_databaseExecutor);

        _winnerPlayerId = CreateRegisteredPlayer(WinnerUsername, WinnerEmail);
        _loserPlayerId = CreateRegisteredPlayer(LoserUsername, LoserEmail);
        _guestPlayerId = _playerRepository.CreateGuest(GuestUsername).PlayerId;
        Dictionary<int, int> totalScoresByPlayerId = new Dictionary<int, int>
        {
            { _winnerPlayerId, WinnerScore },
            { _loserPlayerId, LoserScore },
            { _guestPlayerId, GuestScore }
        };
        _finishedMatch = new FinishedMatchRecord(LobbyCode, MatchRanking.RankByLowestScore(totalScoresByPlayerId));
    }

    [TearDown]
    public void TearDown()
    {
        _transaction.Dispose();
    }

    [Test]
    public void Test_Constructor_NullDatabaseExecutor_ThrowsArgumentNullException()
    {
        _databaseExecutor = null;

        Assert.Throws<ArgumentNullException>(CreateMatchResultRepository);
    }

    [Test]
    public void Test_SaveMatchResults_FinishedMatch_ReturnsTrue()
    {
        bool wasSaved = _matchResultRepository.SaveMatchResults(_finishedMatch);

        Assert.That(wasSaved, Is.True);
    }

    [Test]
    public void Test_SaveMatchResults_SameLobbyCodeTwice_ReusesLobby()
    {
        _matchResultRepository.SaveMatchResults(_finishedMatch);

        _matchResultRepository.SaveMatchResults(_finishedMatch);

        int lobbyCount = _databaseExecutor.Execute(CountLobbiesByCode, LobbyCode);
        Assert.That(lobbyCount, Is.EqualTo(SingleCount));
    }

    [Test]
    public void Test_SaveMatchResults_NullMatch_ThrowsArgumentNullException()
    {
        _finishedMatch = null;

        Assert.Throws<ArgumentNullException>(SaveArrangedMatch);
    }

    [Test]
    public void Test_SaveMatchResults_UnknownPlayer_ThrowsDataIntegrityException()
    {
        _finishedMatch = new FinishedMatchRecord(
            LobbyCode,
            new List<PlayerMatchResult>
            {
                new PlayerMatchResult(UnknownPlayerId, MatchRanking.FirstRank, WinnerScore)
            });

        Assert.Throws<DataIntegrityException>(SaveArrangedMatch);
    }

    [Test]
    public void Test_GetProfileStatisticsByPlayerId_TwoMatchesPlayed_ReturnsGamesPlayed()
    {
        SaveArrangedMatchTwice();

        PlayerProfileStatistics statistics = _matchResultRepository.GetProfileStatisticsByPlayerId(_loserPlayerId);

        Assert.That(statistics.GamesPlayed, Is.EqualTo(ExpectedMatchCount));
    }

    [Test]
    public void Test_GetProfileStatisticsByPlayerId_TwoMatchesWon_ReturnsWins()
    {
        SaveArrangedMatchTwice();

        PlayerProfileStatistics statistics = _matchResultRepository.GetProfileStatisticsByPlayerId(_winnerPlayerId);

        Assert.That(statistics.Wins, Is.EqualTo(ExpectedMatchCount));
    }

    [Test]
    public void Test_GetProfileStatisticsByPlayerId_MatchLost_ReturnsNoWins()
    {
        _matchResultRepository.SaveMatchResults(_finishedMatch);

        PlayerProfileStatistics statistics = _matchResultRepository.GetProfileStatisticsByPlayerId(_loserPlayerId);

        Assert.That(statistics.Wins, Is.Zero);
    }

    [Test]
    public void Test_GetProfileStatisticsByPlayerId_TwoMatchesPlayed_AddsTotalPoints()
    {
        SaveArrangedMatchTwice();

        PlayerProfileStatistics statistics = _matchResultRepository.GetProfileStatisticsByPlayerId(_winnerPlayerId);

        Assert.That(statistics.TotalPoints, Is.EqualTo(WinnerPointsAfterTwoMatches));
    }

    [Test]
    public void Test_GetProfileStatisticsByPlayerId_NoMatchesPlayed_ReturnsEmptyStatistics()
    {
        PlayerProfileStatistics statistics = _matchResultRepository.GetProfileStatisticsByPlayerId(_winnerPlayerId);

        Assert.That(statistics.IsEmpty, Is.True);
    }

    [Test]
    public void Test_GetProfileStatisticsByPlayerId_GuestPlayed_ReturnsEmptyStatistics()
    {
        _matchResultRepository.SaveMatchResults(_finishedMatch);

        PlayerProfileStatistics statistics = _matchResultRepository.GetProfileStatisticsByPlayerId(_guestPlayerId);

        Assert.That(statistics.IsEmpty, Is.True);
    }

    [Test]
    public void Test_GetStatisticsByMostWins_MatchSaved_ListsWinnerFirst()
    {
        _matchResultRepository.SaveMatchResults(_finishedMatch);

        IReadOnlyList<PlayerStatistics> leaderboard = _matchResultRepository.GetStatisticsByMostWins(LeaderboardLimit);

        Assert.That(leaderboard.First().Username, Is.EqualTo(WinnerUsername));
    }

    [Test]
    public void Test_GetStatisticsByMostWins_MatchSaved_CountsWinnerWins()
    {
        _matchResultRepository.SaveMatchResults(_finishedMatch);

        IReadOnlyList<PlayerStatistics> leaderboard = _matchResultRepository.GetStatisticsByMostWins(LeaderboardLimit);

        Assert.That(leaderboard.First().Wins, Is.EqualTo(SingleCount));
    }

    [Test]
    public void Test_GetStatisticsByMostWins_GuestPlayed_ExcludesGuest()
    {
        _matchResultRepository.SaveMatchResults(_finishedMatch);

        IReadOnlyList<PlayerStatistics> leaderboard = _matchResultRepository.GetStatisticsByMostWins(LeaderboardLimit);

        Assert.That(leaderboard, Has.None.Property(UsernamePropertyName).EqualTo(GuestUsername));
    }

    [Test]
    public void Test_GetStatisticsByMostWins_LimitOfOne_ReturnsOneEntry()
    {
        _matchResultRepository.SaveMatchResults(_finishedMatch);

        IReadOnlyList<PlayerStatistics> leaderboard = _matchResultRepository.GetStatisticsByMostWins(SingleCount);

        Assert.That(leaderboard, Has.Count.EqualTo(SingleCount));
    }

    private static int CountLobbiesByCode(DominoGameDBEntities context, string lobbyCode)
    {
        int lobbyCount = (from lobby in context.Lobbies
                          where lobby.Token == lobbyCode
                          select lobby).Count();
        return lobbyCount;
    }

    private int CreateRegisteredPlayer(string username, string email)
    {
        _playerRepository.CreatePlayer(new PlayerRegistration(username, email, PasswordHash));
        int playerId = _playerRepository.FindCredentials(username).PlayerId;
        return playerId;
    }

    private void CreateMatchResultRepository()
    {
        _matchResultRepository = new MatchResultRepository(_databaseExecutor);
    }

    private void SaveArrangedMatch()
    {
        _matchResultRepository.SaveMatchResults(_finishedMatch);
    }

    private void SaveArrangedMatchTwice()
    {
        SaveArrangedMatch();
        SaveArrangedMatch();
    }
}
