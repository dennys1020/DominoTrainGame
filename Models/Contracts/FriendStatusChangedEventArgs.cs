using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class FriendStatusChangedEventArgs : EventArgs
{
    public FriendStatusChangedEventArgs(int playerId, FriendStatus status)
    {
        PlayerId = playerId;
        Status = status;
    }

    public int PlayerId { get; }

    public FriendStatus Status { get; }
}
