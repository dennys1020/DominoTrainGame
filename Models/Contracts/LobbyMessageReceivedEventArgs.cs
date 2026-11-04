using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyMessageReceivedEventArgs : EventArgs
{
    public LobbyMessageReceivedEventArgs(LobbyMember sender, string text)
    {
        Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        Text = text ?? string.Empty;
    }

    public LobbyMember Sender { get; }

    public string Text { get; }
}
