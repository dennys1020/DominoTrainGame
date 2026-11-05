using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class LobbyMessageNotice
{
    public LobbyMessageNotice(IReadOnlyList<int> recipientPlayerIds, LobbyMember sender, string text)
    {
        RecipientPlayerIds = recipientPlayerIds ?? throw new ArgumentNullException(nameof(recipientPlayerIds));
        Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        Text = text ?? string.Empty;
    }

    public IReadOnlyList<int> RecipientPlayerIds { get; }

    public LobbyMember Sender { get; }

    public string Text { get; }
}
