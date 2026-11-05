using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class FriendStatusNotice
{
    public FriendStatusNotice(IReadOnlyList<int> recipientPlayerIds, int playerId, PlayerStatus status)
    {
        RecipientPlayerIds = recipientPlayerIds ?? throw new ArgumentNullException(nameof(recipientPlayerIds));
        PlayerId = playerId;
        Status = status;
    }

    public IReadOnlyList<int> RecipientPlayerIds { get; }

    public int PlayerId { get; }

    public PlayerStatus Status { get; }
}
