using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class FriendListResult
{
    public FriendListResult(FriendActionResult status, IReadOnlyList<FriendResult> friends)
    {
        Status = status;
        Friends = friends ?? throw new ArgumentNullException(nameof(friends));
    }

    public FriendActionResult Status { get; }

    public IReadOnlyList<FriendResult> Friends { get; }
}
