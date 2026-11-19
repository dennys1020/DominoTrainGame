using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LeaderboardResult
{
    public LeaderboardResult(LeaderboardStatus status, IReadOnlyList<LeaderboardEntryResult> entries)
    {
        Status = status;
        Entries = entries ?? throw new ArgumentNullException(nameof(entries));
    }

    public LeaderboardStatus Status { get; }

    public IReadOnlyList<LeaderboardEntryResult> Entries { get; }

    public static LeaderboardResult CreateFailure(LeaderboardStatus status)
    {
        LeaderboardResult result = new LeaderboardResult(status, new List<LeaderboardEntryResult>());
        return result;
    }
}
