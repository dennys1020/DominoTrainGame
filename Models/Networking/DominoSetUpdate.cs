using System;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal sealed class DominoSetUpdate
{
    private readonly DominoSetChangedMessage _message;

    public DominoSetUpdate(DominoSetChangedMessage message)
    {
        _message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public LobbyResult Apply(LobbyResult lobby)
    {
        if (lobby == null)
        {
            throw new ArgumentNullException(nameof(lobby));
        }

        LobbyRoster roster = new LobbyRoster(_message.Version, lobby.HostPlayerId, lobby.Players);
        LobbyResult updatedLobby = lobby.WithDominoSet(_message.HighestDouble, _message.MaximumPlayers, roster);
        return updatedLobby;
    }
}
