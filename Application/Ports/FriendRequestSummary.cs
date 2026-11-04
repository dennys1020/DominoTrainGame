using System;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class FriendRequestSummary
{
    public FriendRequestSummary(int requestId, string requesterUsername, DateTime requestedAt)
    {
        RequestId = requestId;
        RequesterUsername = requesterUsername;
        RequestedAt = requestedAt;
    }

    public int RequestId { get; }
    public string RequesterUsername { get; }
    public DateTime RequestedAt { get; }
}
