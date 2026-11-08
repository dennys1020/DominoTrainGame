using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyMessageReceivedEventArgs : EventArgs
{
    public LobbyMessageReceivedEventArgs(PlayerSummary sender, string text)
    {
        Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        Text = text ?? string.Empty;
    }

    public PlayerSummary Sender { get; }

    public string Text { get; }
}
