namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LeaderboardEntryResult
{
    public LeaderboardEntryResult(int rank, string username, int wins, int gamesPlayed)
    {
        Rank = rank;
        Username = username ?? string.Empty;
        Wins = wins;
        GamesPlayed = gamesPlayed;
    }

    public int Rank { get; }

    public string Username { get; }

    public int Wins { get; }

    public int GamesPlayed { get; }
}
