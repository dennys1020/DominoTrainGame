using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoTrainGame.Server.Application.Domain;

public static class MatchRanking
{
    public const int FirstRank = 1;

    public static IReadOnlyList<PlayerMatchResult> RankByLowestScore(
        IReadOnlyDictionary<int, int> totalScoresByPlayerId)
    {
        if (totalScoresByPlayerId == null)
        {
            throw new ArgumentNullException(nameof(totalScoresByPlayerId));
        }

        List<PlayerMatchResult> playerResults = new List<PlayerMatchResult>();

        foreach (KeyValuePair<int, int> playerScore in totalScoresByPlayerId)
        {
            int finalRank = CalculateRank(totalScoresByPlayerId, playerScore.Value);
            playerResults.Add(new PlayerMatchResult(playerScore.Key, finalRank, playerScore.Value));
        }

        return playerResults;
    }

    public static IReadOnlyList<LeaderboardEntry> RankByMostWins(
        IReadOnlyList<PlayerStatistics> statisticsByMostWins)
    {
        if (statisticsByMostWins == null)
        {
            throw new ArgumentNullException(nameof(statisticsByMostWins));
        }

        List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();

        foreach (PlayerStatistics statistics in statisticsByMostWins)
        {
            leaderboard.Add(new LeaderboardEntry(CalculateWinsRank(leaderboard, statistics), statistics));
        }

        return leaderboard;
    }

    private static int CalculateRank(IReadOnlyDictionary<int, int> totalScoresByPlayerId, int totalScore)
    {
        int finalRank = FirstRank;

        foreach (int otherTotalScore in totalScoresByPlayerId.Values)
        {
            if (otherTotalScore < totalScore)
            {
                finalRank++;
            }
        }

        return finalRank;
    }

    private static int CalculateWinsRank(List<LeaderboardEntry> rankedEntries, PlayerStatistics statistics)
    {
        bool tiesPreviousEntry = rankedEntries.Any() && rankedEntries.Last().Statistics.Wins == statistics.Wins;
        int position = rankedEntries.Count + FirstRank;
        int rank = tiesPreviousEntry ? rankedEntries.Last().Rank : position;

        return rank;
    }
}
