using System;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class FriendPresence
{
    public FriendPresence(FriendSummary friend, PlayerStatus status)
    {
        if (friend == null)
        {
            throw new ArgumentNullException(nameof(friend));
        }

        PlayerId = friend.PlayerId;
        Username = friend.Username ?? string.Empty;
        Status = status;
    }

    public int PlayerId { get; }

    public string Username { get; }

    public PlayerStatus Status { get; }
}
