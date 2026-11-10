using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class MatchPlayerSummary
{
    public MatchPlayerSummary(PlayerSummary player, int handCount)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        HandCount = handCount;
    }

    public PlayerSummary Player { get; }

    public int HandCount { get; }
}
