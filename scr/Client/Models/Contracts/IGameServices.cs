using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.DominoTrainService;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IGameServices
{
    Task<bool> LogInAsync(string identifier, string password);
    Task<bool> RegisterUserAsync(User user);
    Task<List<LeaderboardEntry>> GetEntriesAsync(CancellationToken cancellationToken);
}
