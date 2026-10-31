using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class ConnectionStateChangedEventArgs : EventArgs
{
    public ConnectionStateChangedEventArgs(ConnectionState state)
    {
        State = state;
    }

    public ConnectionState State { get; }
}
