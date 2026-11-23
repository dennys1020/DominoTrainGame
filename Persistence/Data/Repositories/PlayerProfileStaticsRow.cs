namespace DominoTrainGame.Server.Persistence.Data.Repositories;

internal sealed class PlayerProfileStatisticsRow
{
    public int GamesPlayed { get; set; }

    public int Wins { get; set; }

    public int TotalPoints { get; set; }
}
