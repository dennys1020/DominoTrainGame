using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class PlayerSearchResult
{
    public PlayerSearchResult(FriendResultStatus status, IReadOnlyList<FoundPlayerResult> players)
    {
        Status = status;
        Players = players ?? throw new ArgumentNullException(nameof(players));
    }

    public FriendResultStatus Status { get; }

    public IReadOnlyList<FoundPlayerResult> Players { get; }
}
