namespace DominoTrainGame.Server.Persistence.Data.Repositories;

internal sealed class PlayerStatisticsRow
{
    public string Username { get; set; }

    public int Wins { get; set; }

    public int GamesPlayed { get; set; }
}
