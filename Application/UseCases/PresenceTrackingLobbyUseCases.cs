using System;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class PresenceTrackingLobbyUseCases : ILobbyUseCases
{
    private readonly ILobbyUseCases _lobbyUseCases;
    private readonly IPresenceUseCases _presenceUseCases;
    private readonly IFriendStatusUseCases _friendStatusUseCases;

    public PresenceTrackingLobbyUseCases(
        ILobbyUseCases lobbyUseCases,
        IPresenceUseCases presenceUseCases,
        IFriendStatusUseCases friendStatusUseCases)
    {
        _lobbyUseCases = lobbyUseCases ?? throw new ArgumentNullException(nameof(lobbyUseCases));
        _presenceUseCases = presenceUseCases ?? throw new ArgumentNullException(nameof(presenceUseCases));
        _friendStatusUseCases = friendStatusUseCases
            ?? throw new ArgumentNullException(nameof(friendStatusUseCases));
    }

    public bool TryCreateLobby(LobbyMember host, out LobbyState lobby)
    {
        bool wasCreated = _lobbyUseCases.TryCreateLobby(host, out lobby);

        if (wasCreated)
        {
            _friendStatusUseCases.PublishStatus(host.PlayerId);
        }

        return wasCreated;
    }

    public JoinLobbyStatus JoinLobby(LobbyMember member, string code, out LobbyState lobby)
    {
        JoinLobbyStatus status = _lobbyUseCases.JoinLobby(member, code, out lobby);

        if (status == JoinLobbyStatus.Success)
        {
            _friendStatusUseCases.PublishStatus(member.PlayerId);
        }

        return status;
    }

    public void LeaveLobby(int playerId)
    {
        bool wasInLobby = _presenceUseCases.TryFindLobby(playerId, out LobbyState _);
        _lobbyUseCases.LeaveLobby(playerId);

        if (wasInLobby)
        {
            _friendStatusUseCases.PublishStatus(playerId);
        }
    }

    public bool TrySetReady(int playerId, bool isReady)
    {
        bool wasUpdated = _lobbyUseCases.TrySetReady(playerId, isReady);
        return wasUpdated;
    }

    public bool TrySendMessage(int playerId, string text)
    {
        bool wasSent = _lobbyUseCases.TrySendMessage(playerId, text);
        return wasSent;
    }

    public StartMatchStatus StartMatch(int playerId)
    {
        StartMatchStatus status = _lobbyUseCases.StartMatch(playerId);
        return status;
    }
}
