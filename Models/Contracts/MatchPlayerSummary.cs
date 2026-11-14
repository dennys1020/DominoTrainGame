using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class MatchPlayerSummary
{
    public MatchPlayerSummary(PlayerSummary player, int handCount, int score)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        HandCount = handCount;
        Score = score;
    }

    public PlayerSummary Player { get; }

    public int HandCount { get; }

    public int Score { get; }
}
