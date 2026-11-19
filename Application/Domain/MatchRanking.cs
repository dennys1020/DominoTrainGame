using System;
using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Domain;

public static class MatchRanking
{
    private const int FirstRank = 1;

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
}
