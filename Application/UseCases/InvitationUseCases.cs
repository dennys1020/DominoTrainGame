using System;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.State;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class InvitationUseCases : IInvitationUseCases
{
    private static readonly ITraceLog _logger;

    private readonly LobbyRegistry _lobbyRegistry;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IGameNotifier _gameNotifier;

    static InvitationUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(InvitationUseCases));
    }

    public InvitationUseCases(
        LobbyRegistry lobbyRegistry,
        IFriendshipRepository friendshipRepository,
        IGameNotifier gameNotifier)
    {
        _lobbyRegistry = lobbyRegistry ?? throw new ArgumentNullException(nameof(lobbyRegistry));
        _friendshipRepository = friendshipRepository
            ?? throw new ArgumentNullException(nameof(friendshipRepository));
        _gameNotifier = gameNotifier ?? throw new ArgumentNullException(nameof(gameNotifier));
    }

    public InviteFriendStatus InviteFriend(PlayerIdentity inviter, string friendUsername)
    {
        if (inviter == null)
        {
            throw new ArgumentNullException(nameof(inviter));
        }

        string normalizedUsername = PlayerInputValidator.NormalizeUsername(friendUsername);

        LobbyState lobby = _lobbyRegistry.FindLobbyOfPlayer(inviter.PlayerId);
        InviteFriendStatus status = lobby.IsEmpty
            ? InviteFriendStatus.LobbyNotFound
            : InviteToLobby(inviter, normalizedUsername, lobby);

        LogInvitation(inviter.PlayerId, status);
        return status;
    }

    private InviteFriendStatus InviteToLobby(PlayerIdentity inviter, string friendUsername, LobbyState lobby)
    {
        InviteFriendStatus status = InviteFriendStatus.LobbyFull;

        if (!lobby.IsFull)
        {
            status = InviteRegisteredPlayer(inviter, friendUsername, lobby.Code);
        }

        return status;
    }

    private InviteFriendStatus InviteRegisteredPlayer(PlayerIdentity inviter, string friendUsername, string code)
    {
        PlayerIdentity friend = _friendshipRepository.FindRegisteredPlayer(friendUsername);
        InviteFriendStatus status = friend.IsEmpty
            ? InviteFriendStatus.PlayerNotFound
            : InviteFriendPlayer(inviter, friend.PlayerId, code);

        return status;
    }

    private InviteFriendStatus InviteFriendPlayer(PlayerIdentity inviter, int friendPlayerId, string code)
    {
        InviteFriendStatus status = InviteFriendStatus.NotFriends;
        FriendshipState friendshipState = _friendshipRepository.GetFriendshipState(inviter.PlayerId, friendPlayerId);

        if (friendshipState == FriendshipState.Accepted)
        {
            LobbyInvitationNotice notice = new LobbyInvitationNotice(friendPlayerId, inviter, code);
            bool wasDelivered = _gameNotifier.TryNotifyLobbyInvitation(notice);
            status = wasDelivered ? InviteFriendStatus.Success : InviteFriendStatus.PlayerOffline;
        }

        return status;
    }

    private static void LogInvitation(int inviterPlayerId, InviteFriendStatus status)
    {
        if (status == InviteFriendStatus.Success)
        {
            _logger.InfoFormat("A lobby invitation was sent. InviterPlayerId: {0}.", inviterPlayerId);
        }
        else
        {
            _logger.WarnFormat(
                "A lobby invitation was rejected. InviterPlayerId: {0}. Status: {1}.",
                inviterPlayerId,
                status);
        }
    }
}
