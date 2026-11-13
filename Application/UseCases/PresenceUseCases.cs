using System;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.State;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class PresenceUseCases : IPresenceUseCases
{
    private readonly IPresenceTracker _presenceTracker;
    private readonly LobbyRegistry _lobbyRegistry;

    public PresenceUseCases(IPresenceTracker presenceTracker, LobbyRegistry lobbyRegistry)
    {
        _presenceTracker = presenceTracker ?? throw new ArgumentNullException(nameof(presenceTracker));
        _lobbyRegistry = lobbyRegistry ?? throw new ArgumentNullException(nameof(lobbyRegistry));
    }

    public PlayerStatus GetStatus(int playerId)
    {
        bool isInLobby = !_lobbyRegistry.FindLobbyOfPlayer(playerId).IsEmpty;
        bool isOnline = _presenceTracker.IsOnline(playerId);
        PlayerStatus status = PlayerStatus.Offline;

        if (isInLobby)
        {
            status = PlayerStatus.InLobby;
        }
        else if (isOnline)
        {
            status = PlayerStatus.Online;
        }

        return status;
    }
}
