using System.Collections.Generic;
using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Application.Results;

public sealed class LeaderboardResultSet
{
    public bool Succeeded { get; set; }
    public List<LeaderboardRecord> Entries { get; set; } = new List<LeaderboardRecord>();
}
