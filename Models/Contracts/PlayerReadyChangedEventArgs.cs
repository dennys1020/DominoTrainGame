using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class PlayerReadyChangedEventArgs : EventArgs
{
    public PlayerReadyChangedEventArgs(int playerId, bool isReady, long version)
    {
        PlayerId = playerId;
        IsReady = isReady;
        Version = version;
    }

    public int PlayerId { get; }

    public bool IsReady { get; }

    public long Version { get; }
}
