using System;
using System.ServiceModel;
using DominoTrainGame.Client.Models.DominoTrainService;
using DominoTrainGame.Client.Models.Errors;

namespace DominoTrainGame.Client.Models.Callbacks;

[CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Single)]
internal sealed class LeaderboardCallback : OperationCallback<LeaderboardResponse>, ILeaderboardServiceCallback
{
    public LeaderboardCallback(Guid requestId, Action<Guid, OperationState, string> report) : base(requestId, report) { }

    public void OnLeaderboardChanged(LeaderboardNotification notification)
    {
        if (notification is null) { Fail(new GameServiceException("The server returned an empty leaderboard notification.")); }
        else { Receive(notification.RequestId, notification.State, notification.Message, notification.Result); }
    }
}
