using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class PlayerJoinedEventArgs : EventArgs
{
    public PlayerJoinedEventArgs(LobbyPlayerResult player, long version)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Version = version;
    }

    public LobbyPlayerResult Player { get; }

    public long Version { get; }
}
