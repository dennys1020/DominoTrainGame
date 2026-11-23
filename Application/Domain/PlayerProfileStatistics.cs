namespace DominoTrainGame.Server.Application.Domain;

public sealed class PlayerProfileStatistics
{
    private const int EmptyStatisticValue = 0;

    static PlayerProfileStatistics()
    {
        Empty = new PlayerProfileStatistics(EmptyStatisticValue, EmptyStatisticValue, EmptyStatisticValue);
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
