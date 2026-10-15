using System.Linq;
using System.Threading.Tasks;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed partial class GameService : ILeaderboardService
{
    public async Task<LeaderboardResponse> GetLeaderboardAsync()
    {
        LeaderboardResultSet result = await _application.GetLeaderboardAsync().ConfigureAwait(false);
        return new LeaderboardResponse
        {
            Succeeded = result.Succeeded,
            Entries = result.Entries.Select(entry => new LeaderboardEntry
            {
                Rank = entry.Rank, PlayerId = entry.PlayerId, Username = entry.Username,
                Wins = entry.Wins, GamesPlayed = entry.GamesPlayed, TotalScore = entry.TotalScore
            }).ToList()
        };
    }
}
