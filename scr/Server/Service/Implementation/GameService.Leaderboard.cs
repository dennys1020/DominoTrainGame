using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Errors;
using DominoTrainGame.Server.Persistence.Models;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed partial class GameService : ILeaderboardService
{
    private readonly ILeaderboardSource _leaderboardSource;

    public async Task GetLeaderboardAsync(Guid requestId)
    {
        ILeaderboardCallback callback = OperationContext.Current.GetCallbackChannel<ILeaderboardCallback>();
        LeaderboardResponse response = await LoadLeaderboardAsync().ConfigureAwait(false);
        NotifyClient(() => callback.OnLeaderboardReceived(requestId, response));
    }

    private async Task<LeaderboardResponse> LoadLeaderboardAsync()
    {
        LeaderboardResponse response = new LeaderboardResponse();

        try
        {
            List<LeaderboardRecord> entries =
                await _leaderboardSource
                    .GetEntriesAsync(CancellationToken.None)
                    .ConfigureAwait(false);

            foreach (LeaderboardRecord entry in entries)
            {
                LeaderboardEntry leaderboardEntry = new LeaderboardEntry
                {
                    Rank = entry.Rank,
                    PlayerId = entry.PlayerId,
                    Username = entry.Username,
                    Wins = entry.Wins,
                    GamesPlayed = entry.GamesPlayed,
                    TotalScore = entry.TotalScore
                };

                response.Entries.Add(leaderboardEntry);
            }

            response.Succeeded = true;
        }
        catch (DatabaseOperationException)
        {
            response.Succeeded = false;
        }

        return response;
    }
}
