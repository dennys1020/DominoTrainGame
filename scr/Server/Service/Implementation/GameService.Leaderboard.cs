using System;
using System.Linq;
using System.ServiceModel;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed partial class GameService : ILeaderboardService
{
    public void GetLeaderboard(LeaderboardRequest request)
    {
        ILeaderboardCallback callback = OperationContext.Current.GetCallbackChannel<ILeaderboardCallback>();
        Guid requestId = request?.RequestId ?? Guid.Empty;
        RunOneWay(async () =>
        {
            if (!NotifyLeaderboard(callback, requestId, OperationState.Started, "Loading the leaderboard."))
            {
                return;
            }
            if (request is null)
            {
                NotifyLeaderboard(callback, requestId, OperationState.Failed, "The leaderboard request is missing.");
                return;
            }
            LeaderboardResultSet result = await _application.GetLeaderboardAsync().ConfigureAwait(false);
            NotifyLeaderboard(callback, requestId, result.Succeeded ? OperationState.Completed : OperationState.Failed,
                result.Succeeded ? "The leaderboard was loaded." : "The leaderboard could not be loaded.",
                new LeaderboardResponse
                {
                    Succeeded = result.Succeeded,
                    Entries = result.Entries.Select(entry => new LeaderboardEntry
                    {
                        Rank = entry.Rank, PlayerId = entry.PlayerId, Username = entry.Username,
                        Wins = entry.Wins, GamesPlayed = entry.GamesPlayed, TotalScore = entry.TotalScore
                    }).ToList()
                });
        }, _ => NotifyLeaderboard(callback, requestId, OperationState.Failed,
            "The server could not load the leaderboard."));
    }

    private static bool NotifyLeaderboard(ILeaderboardCallback callback, Guid requestId, OperationState state,
        string message, LeaderboardResponse result = null) => NotifyClient(() =>
            callback.OnLeaderboardChanged(new LeaderboardNotification
            {
                RequestId = requestId, State = state, Message = message, Result = result
            }));
}
