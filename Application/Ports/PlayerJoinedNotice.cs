using System;
using System.Collections.Generic;

using DominoTrainGame.Server.Application.Connection;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class PlayerJoinedNotice
{
    public PlayerJoinedNotice(IReadOnlyList<int> recipientPlayerIds, LobbyMember member, long version)
    {
        RecipientPlayerIds = recipientPlayerIds ?? throw new ArgumentNullException(nameof(recipientPlayerIds));
        Member = member ?? throw new ArgumentNullException(nameof(member));
        Version = version;
    }

    public IReadOnlyList<int> RecipientPlayerIds { get; }

    public LobbyMember Member { get; }

    public long Version { get; }
}
