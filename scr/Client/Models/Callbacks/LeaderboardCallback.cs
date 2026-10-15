using System;
using System.ServiceModel;
using DominoTrainGame.Client.Models.DominoTrainService;

namespace DominoTrainGame.Client.Models.Callbacks;

[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
internal sealed class LeaderboardCallback : ILeaderboardServiceCallback
{
    public PendingResponse<LeaderboardResponse> Entries { get; } = new PendingResponse<LeaderboardResponse>();

    public void OnLeaderboardReceived(Guid requestId, LeaderboardResponse response)
    {
        Entries.Complete(requestId, response);
    }
}
