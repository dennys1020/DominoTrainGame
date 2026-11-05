using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class PlayerReadyChangedNotice
{
    public PlayerReadyChangedNotice(
        IReadOnlyList<int> recipientPlayerIds,
        LobbyPlayerState player,
        long version)
    {
        RecipientPlayerIds = recipientPlayerIds ?? throw new ArgumentNullException(nameof(recipientPlayerIds));
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Version = version;
    }

    public IReadOnlyList<int> RecipientPlayerIds { get; }

    public LobbyPlayerState Player { get; }

    public long Version { get; }
}
