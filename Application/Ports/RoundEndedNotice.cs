using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class RoundEndedNotice
{
    public RoundEndedNotice(
        IReadOnlyList<int> recipientPlayerIds,
        int enginePips,
        RoundEndReason endReason,
        IReadOnlyDictionary<int, int> roundScoresByPlayerId)
    {
        RecipientPlayerIds = recipientPlayerIds ?? throw new ArgumentNullException(nameof(recipientPlayerIds));
        EnginePips = enginePips;
        EndReason = endReason;
        RoundScoresByPlayerId = roundScoresByPlayerId
            ?? throw new ArgumentNullException(nameof(roundScoresByPlayerId));
    }

    public IReadOnlyList<int> RecipientPlayerIds { get; }

    public int EnginePips { get; }

    public RoundEndReason EndReason { get; }

    public IReadOnlyDictionary<int, int> RoundScoresByPlayerId { get; }
}
