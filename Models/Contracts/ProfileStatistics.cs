namespace DominoTrainGame.Client.Models.Contracts;

public sealed class ProfileStatistics
{
    private const int NoRecordedGames = 0;

    static ProfileStatistics()
    {
        Empty = new ProfileStatistics(NoRecordedGames, NoRecordedGames, NoRecordedGames);
    }

    public ProfileStatistics(int gamesPlayed, int wins, int totalPoints)
    {
        GamesPlayed = gamesPlayed;
        Wins = wins;
        TotalPoints = totalPoints;
    }

    public static ProfileStatistics Empty { get; }

    public int GamesPlayed { get; }

    public int Wins { get; }

    public int TotalPoints { get; }
}
