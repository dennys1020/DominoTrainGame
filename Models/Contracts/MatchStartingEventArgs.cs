using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class MatchStartingEventArgs : EventArgs
{
    public MatchStartingEventArgs(string code)
    {
        Code = code ?? string.Empty;
    }

    public string Code { get; }
}
