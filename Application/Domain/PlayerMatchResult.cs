namespace DominoTrainGame.Server.Application.Domain;

public sealed class PlayerMatchResult
{
    public PlayerMatchResult(int playerId, int finalRank, int totalScore)
    {
        PlayerId = playerId;
        FinalRank = finalRank;
        TotalScore = totalScore;
    }

    public int PlayerId { get; }

    public int FinalRank { get; }

    public int TotalScore { get; }
}
