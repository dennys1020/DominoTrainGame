namespace DominoTrainGame.Server.Application.Domain;

public sealed class PlayerProfileStatistics
{
    private const int NoGames = 0;
    private const int NoWins = 0;
    private const int NoPoints = 0;

    static PlayerProfileStatistics()
    {
        Empty = new PlayerProfileStatistics(NoGames, NoWins, NoPoints);
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
