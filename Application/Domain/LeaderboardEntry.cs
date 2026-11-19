using System;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class LeaderboardEntry
{
    public LeaderboardEntry(int rank, PlayerStatistics statistics)
    {
        Rank = rank;
        Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
    }

    public int Rank { get; }

    public PlayerStatistics Statistics { get; }
}
