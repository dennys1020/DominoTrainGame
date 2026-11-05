using System;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class LobbyInvitationNotice
{
    public LobbyInvitationNotice(int recipientPlayerId, LobbyMember inviter, string code)
    {
        RecipientPlayerId = recipientPlayerId;
        Inviter = inviter ?? throw new ArgumentNullException(nameof(inviter));
        Code = code ?? string.Empty;
    }

    public int RecipientPlayerId { get; }

    public LobbyMember Inviter { get; }

    public string Code { get; }
}
