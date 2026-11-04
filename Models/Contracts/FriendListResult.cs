using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class FriendListResult
{
    public FriendListResult(FriendResultStatus status, IReadOnlyList<FriendResult> friends)
    {
        Status = status;
        Friends = friends ?? throw new ArgumentNullException(nameof(friends));
    }

    public FriendResultStatus Status { get; }

    public IReadOnlyList<FriendResult> Friends { get; }
}
