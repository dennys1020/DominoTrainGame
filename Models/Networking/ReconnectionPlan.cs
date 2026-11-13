using System;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class ReconnectionPlan
{
    public ReconnectionPlan(bool canReconnect, string lobbyCode)
    {
        CanReconnect = canReconnect;
        LobbyCode = lobbyCode ?? throw new ArgumentNullException(nameof(lobbyCode));
    }

    public bool CanReconnect { get; }

    public string LobbyCode { get; }
}
