using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class PlayerLeftEventArgs : EventArgs
{
    public PlayerLeftEventArgs(int playerId, long version, int hostPlayerId)
    {
        PlayerId = playerId;
        Version = version;
        HostPlayerId = hostPlayerId;
    }

    public int PlayerId { get; }

    public long Version { get; }

    public int HostPlayerId { get; }
}
