using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyInvitationReceivedEventArgs : EventArgs
{
    public LobbyInvitationReceivedEventArgs(string inviterUsername, string code)
    {
        InviterUsername = inviterUsername ?? string.Empty;
        Code = code ?? string.Empty;
    }

    public string InviterUsername { get; }

    public string Code { get; }
}
