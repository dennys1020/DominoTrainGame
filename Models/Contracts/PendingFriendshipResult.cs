using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class PendingFriendshipResult
{
    public PendingFriendshipResult(int requestId, string requesterUsername, DateTime requestedAt)
    {
        RequestId = requestId;
        RequesterUsername = requesterUsername ?? string.Empty;
        RequestedAt = requestedAt;
    }

    public int RequestId { get; }

    public string RequesterUsername { get; }

    public DateTime RequestedAt { get; }
}
