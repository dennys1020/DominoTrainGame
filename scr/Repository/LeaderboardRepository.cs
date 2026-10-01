using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Models;

namespace DominoTrainGame.Repository;

/// <summary>
/// Reads leaderboard statistics from completed matches in the game database.
/// </summary>
public sealed class LeaderboardRepository
{
    private const string FinishedMatchStatus = "Finished";
    private const int WinnerRank = 1;
    private const int FirstRank = 1;

    public async Task<List<LeaderboardEntry>> GetEntriesAsync(CancellationToken cancellationToken)
    {
        using (DominoGameDBEntities databaseContext = new DominoGameDBEntities())
        {
            List<LeaderboardEntry> entries = await databaseContext.MatchResults
                .AsNoTracking()
                .Where(result => result.Match.Status == FinishedMatchStatus)
                .GroupBy(result => new { result.PlayerId, result.Player.userName })
                .Select(results => new LeaderboardEntry
                {
                    PlayerId = results.Key.PlayerId,
                    Username = results.Key.userName,
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