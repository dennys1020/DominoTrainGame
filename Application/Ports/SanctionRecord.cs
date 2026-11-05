using System;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class SanctionRecord
{
    public SanctionRecord(int playerId, PlayerSanction sanction, DateTime startedAt)
    {
        PlayerId = playerId;
        Sanction = sanction ?? throw new ArgumentNullException(nameof(sanction));
        StartedAt = startedAt;
    }

    public int PlayerId { get; }

    public PlayerSanction Sanction { get; }

    public DateTime StartedAt { get; }
}
