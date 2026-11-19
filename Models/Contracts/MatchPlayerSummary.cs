using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class MatchPlayerSummary
{
    public MatchPlayerSummary(PlayerSummary player, int score)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Score = score;
    }

    public PlayerSummary Player { get; }

    public int Score { get; }
}
