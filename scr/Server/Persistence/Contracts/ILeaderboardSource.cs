using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Persistence.Contracts;

public interface ILeaderboardSource
{
    Task<List<LeaderboardRecord>> GetEntriesAsync(CancellationToken cancellationToken);
}
