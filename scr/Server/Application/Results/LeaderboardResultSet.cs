using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Results;

public sealed class LeaderboardResultSet
{
    public bool Succeeded { get; set; }
    public List<LeaderboardResult> Entries { get; set; } = new List<LeaderboardResult>();
}
