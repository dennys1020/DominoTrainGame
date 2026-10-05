using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Models;

namespace DominoTrainGame.Repository;

public sealed class LeaderboardRepository
{
    private const string FinishedMatchStatus = "Finished";
    private const int WinnerRank = 1;
    private const int FirstRank = 1;

    public async Task<List<LeaderboardEntry>> GetEntriesAsync(CancellationToken cancellationToken)
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

    private static async Task<List<LeaderboardEntry>> LoadEntriesAsync(CancellationToken cancellationToken)
    {
        using (DominoGameDBEntities databaseContext = DatabaseContextFactory.Create())
        {
            List<LeaderboardEntry> entries = await databaseContext.MatchResults
                .AsNoTracking()
                .Where(result => result.Match.Status == FinishedMatchStatus)
                .GroupBy(result => new { result.PlayerId, result.Player.Username })
                .Select(results => new LeaderboardEntry
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
            foreach (LeaderboardEntry entry in entries)
            {
                entry.Rank = rank;
                rank++;
            }

            return entries;
        }
    }
}
