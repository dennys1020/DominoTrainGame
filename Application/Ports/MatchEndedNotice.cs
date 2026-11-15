using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class MatchEndedNotice
{
    public MatchEndedNotice(IReadOnlyList<int> recipientPlayerIds, MatchEndReason endReason)
    {
        RecipientPlayerIds = recipientPlayerIds ?? throw new ArgumentNullException(nameof(recipientPlayerIds));
        EndReason = endReason;
    }

    public IReadOnlyList<int> RecipientPlayerIds { get; }

    public MatchEndReason EndReason { get; }
}
