using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class PendingFriendshipListResult
{
    public PendingFriendshipListResult(FriendActionResult status, IReadOnlyList<PendingFriendshipResult> requests)
    {
        Status = status;
        Requests = requests ?? throw new ArgumentNullException(nameof(requests));
    }

    public FriendActionResult Status { get; }

    public IReadOnlyList<PendingFriendshipResult> Requests { get; }
}
