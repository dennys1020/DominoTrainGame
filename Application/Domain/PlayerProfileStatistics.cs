namespace DominoTrainGame.Server.Application.Domain;

public sealed class PlayerProfileStatistics
{
    static PlayerProfileStatistics()
    {
        Empty = new PlayerProfileStatistics(0, 0, 0);
    }

    public PlayerProfileStatistics(int gamesPlayed, int wins, int totalPoints)
    {
        GamesPlayed = gamesPlayed;
        Wins = wins;
        TotalPoints = totalPoints;
    }

    public static PlayerProfileStatistics Empty { get; }

    public int GamesPlayed { get; }

    public int Wins { get; }

    public int TotalPoints { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);
}
