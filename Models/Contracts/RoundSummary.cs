using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class RoundSummary
{
    public RoundSummary(int enginePips, IReadOnlyDictionary<int, int> scoresByPlayerId)
    {
        EnginePips = enginePips;
        ScoresByPlayerId = scoresByPlayerId ?? new Dictionary<int, int>();
    }

    public int EnginePips { get; }

    public IReadOnlyDictionary<int, int> ScoresByPlayerId { get; }
}
