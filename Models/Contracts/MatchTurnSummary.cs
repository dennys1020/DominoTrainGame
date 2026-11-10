namespace DominoTrainGame.Client.Models.Contracts;

public sealed class MatchTurnSummary
{
    public MatchTurnSummary(long version, int currentPlayerId)
    {
        Version = version;
        CurrentPlayerId = currentPlayerId;
    }

    public long Version { get; }

    public int CurrentPlayerId { get; }
}
