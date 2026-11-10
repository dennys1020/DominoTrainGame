using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class MatchTableSummary
{
    public MatchTableSummary(int enginePips, int boneyardCount, IReadOnlyList<TrainSummary> trains)
    {
        if (trains == null)
        {
            throw new ArgumentNullException(nameof(trains));
        }

        EnginePips = enginePips;
        BoneyardCount = boneyardCount;
        Trains = new List<TrainSummary>(trains).AsReadOnly();
    }

    public int EnginePips { get; }

    public int BoneyardCount { get; }

    public IReadOnlyList<TrainSummary> Trains { get; }
}
