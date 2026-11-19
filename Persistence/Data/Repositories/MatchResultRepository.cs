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
    private const string PrivateAccessType = "Private";
    private const string FinishedStatus = "Finished";

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
        List<PlayerStatisticsRow> statisticsRows = _databaseExecutor.Execute(
            QueryStatisticsByMostWins,
            maximumPlayers);
        IReadOnlyList<PlayerStatistics> statisticsByMostWins = ToStatistics(statisticsRows);

        return statisticsByMostWins;
    }

    private static bool InsertFinishedMatch(DominoGameDBEntities context, FinishedMatchRecord finishedMatch)
    {
        bool wasSaved;

        using (DbContextTransaction transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
        {
            Match match = new Match
            {
                Lobby = FindOrCreateLobby(context, finishedMatch),
                Status = FinishedStatus
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

    private static Lobby FindOrCreateLobby(DominoGameDBEntities context, FinishedMatchRecord finishedMatch)
    {
        List<Lobby> savedLobbies = (from lobby in context.Lobbies
                                    where lobby.Token == finishedMatch.LobbyCode
                                    select lobby).ToList();
        Lobby matchLobby = savedLobbies.Any() ? savedLobbies.First() : CreateLobby(finishedMatch.LobbyCode);
        matchLobby.MaxPlayers = Convert.ToByte(finishedMatch.MaximumPlayers);
        matchLobby.Status = FinishedStatus;

        return matchLobby;
    }

    private static Lobby CreateLobby(string lobbyCode)
    {
        Lobby lobby = new Lobby
        {
            AccessType = PrivateAccessType,
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

    private static List<PlayerStatisticsRow> QueryStatisticsByMostWins(
        DominoGameDBEntities context,
        int maximumPlayers)
    {
        List<PlayerStatisticsRow> statisticsRows = (from player in context.Players
                                                    where player.IsGuest != GuestPlayerFlag
                                                        && player.MatchResults.Any()
                                                    let wins = (from matchResult in player.MatchResults
                                                                where matchResult.FinalRank == MatchRanking.FirstRank
                                                                select matchResult).Count()
                                                    let gamesPlayed = player.MatchResults.Count()
                                                    orderby wins descending, gamesPlayed, player.Username
                                                    select new PlayerStatisticsRow
                                                    {
                                                        Username = player.Username,
                                                        Wins = wins,
                                                        GamesPlayed = gamesPlayed
                                                    }).Take(maximumPlayers).ToList();
        return statisticsRows;
    }

    private static IReadOnlyList<PlayerStatistics> ToStatistics(List<PlayerStatisticsRow> statisticsRows)
    {
        List<PlayerStatistics> statisticsByMostWins = new List<PlayerStatistics>();

        foreach (PlayerStatisticsRow statisticsRow in statisticsRows)
        {
            statisticsByMostWins.Add(new PlayerStatistics(
                statisticsRow.Username,
                statisticsRow.Wins,
                statisticsRow.GamesPlayed));
        }

        return statisticsByMostWins;
    }
}
