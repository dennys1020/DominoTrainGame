using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Models;

namespace DominoTrainGame.Server.Application.Ports;

public interface ILeaderboardSource
{
    Task<List<LeaderboardEntry>> GetEntriesAsync(CancellationToken cancellationToken);
}
