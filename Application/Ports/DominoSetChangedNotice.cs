using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class DominoSetChangedNotice
{
    public DominoSetChangedNotice(IReadOnlyList<int> recipientPlayerIds, DominoSet dominoSet, long version)
    {
        RecipientPlayerIds = recipientPlayerIds ?? throw new ArgumentNullException(nameof(recipientPlayerIds));
        DominoSet = dominoSet ?? throw new ArgumentNullException(nameof(dominoSet));
        Version = version;
    }

    public IReadOnlyList<int> RecipientPlayerIds { get; }

    public DominoSet DominoSet { get; }

    public long Version { get; }
}
