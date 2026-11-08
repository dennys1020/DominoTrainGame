using System;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class PlayerSessionEventArgs : EventArgs
{
    public PlayerSessionEventArgs(int playerId)
    {
        PlayerId = playerId;
    }

    public int PlayerId { get; }
}
