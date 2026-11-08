using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class PlayerSearchResult
{
    public PlayerSearchResult(FriendResultStatus status, IReadOnlyList<PlayerSummary> players)
    {
        Status = status;
        Players = players ?? throw new ArgumentNullException(nameof(players));
    }

    public FriendResultStatus Status { get; }

    public IReadOnlyList<PlayerSummary> Players { get; }
}
