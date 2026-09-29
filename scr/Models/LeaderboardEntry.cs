#nullable enable

namespace DominoTrainGame.Models;

public sealed class LeaderboardEntry
{
    public int Rank { get; set; }

    public int PlayerId { get; set; }

    public string Username { get; set; } = string.Empty;

    public int Wins { get; set; }

    public int GamesPlayed { get; set; }

    public long TotalScore { get; set; }
}
