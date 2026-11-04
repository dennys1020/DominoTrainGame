using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class FriendRequestListResult
{
    public FriendRequestListResult(FriendResultStatus status, IReadOnlyList<FriendRequestResult> requests)
    {
        Status = status;
        Requests = requests ?? throw new ArgumentNullException(nameof(requests));
    }

    public FriendResultStatus Status { get; }

    public IReadOnlyList<FriendRequestResult> Requests { get; }
}
