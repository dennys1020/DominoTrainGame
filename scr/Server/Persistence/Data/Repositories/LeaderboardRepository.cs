using DominoTrainGame.Server.Persistence.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Server.Persistence.Models;
using DominoTrainGame.Server.Persistence.Errors;

namespace DominoTrainGame.Server.Persistence.Repositories;

public sealed class LeaderboardRepository : ILeaderboardSource
{
    private const string FinishedMatchStatus = "Finished";
    private const int WinnerRank = 1;
    private const int FirstRank = 1;

    public async Task<List<LeaderboardRecord>> GetEntriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await LoadEntriesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is EntityException || exception is SqlException)
        {
            throw new DatabaseOperationException("The leaderboard database query failed.", exception);
        }
    }

    private static async Task<List<LeaderboardRecord>> LoadEntriesAsync(CancellationToken cancellationToken)
    {
        using (DominoGameDBEntities databaseContext = DatabaseContextFactory.Create())
        {
            List<LeaderboardRecord> entries = await databaseContext.MatchResults
                .AsNoTracking()
                .Where(result => result.Match.Status == FinishedMatchStatus)
                .GroupBy(result => new { result.PlayerId, result.Player.Username })
                .Select(results => new LeaderboardRecord
                {
                    PlayerId = results.Key.PlayerId,
                    Username = results.Key.Username,
                    Wins = results.Count(result => result.FinalRank == WinnerRank),
                    GamesPlayed = results.Count(),
                    TotalScore = results.Sum(result => (long)result.TotalScore)
                })
                .OrderByDescending(entry => entry.Wins)
                .ThenBy(entry => entry.Username)
                .ThenBy(entry => entry.PlayerId)
                .ToListAsync(cancellationToken);

            int rank = FirstRank;
            foreach (LeaderboardRecord entry in entries)
            {
                entry.Rank = rank;
                rank++;
            }

            return entries;
        }
    }
}
