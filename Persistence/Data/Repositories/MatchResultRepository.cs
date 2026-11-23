using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Persistence.Data.Repositories;

public sealed class MatchResultRepository : IMatchResultRepository
{
    private const int NoSavedRows = 0;
    private const byte GuestPlayerFlag = 1;

    private readonly DatabaseExecutor _databaseExecutor;

    public MatchResultRepository(DatabaseExecutor databaseExecutor)
    {
        _databaseExecutor = databaseExecutor ?? throw new ArgumentNullException(nameof(databaseExecutor));
    }

    public bool SaveMatchResults(FinishedMatchRecord finishedMatch)
    {
        if (finishedMatch == null)
        {
            throw new ArgumentNullException(nameof(finishedMatch));
        }

        bool wasSaved = _databaseExecutor.Execute(InsertFinishedMatch, finishedMatch);
        return wasSaved;
    }

    public IReadOnlyList<PlayerStatistics> GetStatisticsByMostWins(int maximumPlayers)
    {
        List<PlayerStatistics> statisticsByMostWins = _databaseExecutor.Execute(
            QueryStatisticsByMostWins,
            maximumPlayers);

        return statisticsByMostWins;
    }

    public PlayerProfileStatistics GetProfileStatisticsByPlayerId(int playerId)
    {
        PlayerProfileStatistics statistics = _databaseExecutor.Execute(QueryProfileStatisticsByPlayerId, playerId);

        return statistics;
    }

    private static PlayerProfileStatistics QueryProfileStatisticsByPlayerId(
        DominoGameDBEntities context,
        int playerId)
    {
        List<MatchResult> playerResults = context.MatchResults
            .AsNoTracking()
            .Where(matchResult => matchResult.PlayerId == playerId)
            .Where(matchResult => matchResult.Player.IsGuest != GuestPlayerFlag)
            .ToList();
        PlayerProfileStatistics statistics = playerResults.Any()
            ? ToProfileStatistics(playerResults)
            : PlayerProfileStatistics.Empty;

        return statistics;
    }

    private static PlayerProfileStatistics ToProfileStatistics(List<MatchResult> playerResults)
    {
        int wins = playerResults.Count(IsFirstRank);
        int totalPoints = playerResults.Sum(matchResult => matchResult.TotalScore);
        PlayerProfileStatistics statistics = new PlayerProfileStatistics(playerResults.Count, wins, totalPoints);

        return statistics;
    }

    private static bool IsFirstRank(MatchResult matchResult)
    {
        bool isFirstRank = matchResult.FinalRank == MatchRanking.FirstRank;
        return isFirstRank;
    }

    private static bool InsertFinishedMatch(DominoGameDBEntities context, FinishedMatchRecord finishedMatch)
    {
        bool wasSaved;

        using (DbContextTransaction transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
        {
            Match match = new Match
            {
                Lobby = FindOrCreateLobby(context, finishedMatch.LobbyCode)
            };

            foreach (PlayerMatchResult playerResult in finishedMatch.PlayerResults)
            {
                match.MatchResults.Add(CreateMatchResult(playerResult));
            }

            context.Matches.Add(match);
            wasSaved = context.SaveChanges() > NoSavedRows;
            transaction.Commit();
        }

        return wasSaved;
    }

    private static Lobby FindOrCreateLobby(DominoGameDBEntities context, string lobbyCode)
    {
        List<Lobby> savedLobbies = context.Lobbies
            .Where(lobby => lobby.Token == lobbyCode)
            .ToList();
        Lobby matchLobby = savedLobbies.Any() ? savedLobbies.First() : CreateLobby(lobbyCode);

        return matchLobby;
    }

    private static Lobby CreateLobby(string lobbyCode)
    {
        Lobby lobby = new Lobby
        {
            Token = lobbyCode
        };
        return lobby;
    }

    private static MatchResult CreateMatchResult(PlayerMatchResult playerResult)
    {
        MatchResult matchResult = new MatchResult
        {
            PlayerId = playerResult.PlayerId,
            FinalRank = playerResult.FinalRank,
            TotalScore = playerResult.TotalScore
        };
        return matchResult;
    }

    private static List<PlayerStatistics> QueryStatisticsByMostWins(
        DominoGameDBEntities context,
        int maximumPlayers)
    {
        List<PlayerStatistics> statisticsByMostWins = context.Players
            .AsNoTracking()
            .Include(player => player.MatchResults)
            .Where(player => player.IsGuest != GuestPlayerFlag)
            .Where(player => player.MatchResults.Any())
            .AsEnumerable()
            .Select(ToPlayerStatistics)
            .OrderByDescending(statistics => statistics.Wins)
            .ThenBy(statistics => statistics.GamesPlayed)
            .ThenBy(statistics => statistics.Username, StringComparer.OrdinalIgnoreCase)
            .Take(maximumPlayers)
            .ToList();

        return statisticsByMostWins;
    }

    private static PlayerStatistics ToPlayerStatistics(Player player)
    {
        int wins = player.MatchResults.Count(IsFirstRank);
        PlayerStatistics statistics = new PlayerStatistics(player.Username, wins, player.MatchResults.Count);

        return statistics;
    }
}
