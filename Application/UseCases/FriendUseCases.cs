using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class FriendUseCases : IFriendUseCases
{
    private static readonly ITraceLog _logger;

    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IPresenceUseCases _presenceUseCases;
    private readonly ISocialNotifier _socialNotifier;

    static FriendUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(FriendUseCases));
    }

    public FriendUseCases(
        IFriendshipRepository friendshipRepository,
        IPresenceUseCases presenceUseCases,
        ISocialNotifier socialNotifier)
    {
        _friendshipRepository = friendshipRepository
            ?? throw new ArgumentNullException(nameof(friendshipRepository));
        _presenceUseCases = presenceUseCases ?? throw new ArgumentNullException(nameof(presenceUseCases));
        _socialNotifier = socialNotifier ?? throw new ArgumentNullException(nameof(socialNotifier));
    }

    public IReadOnlyList<FriendPresence> GetFriends(int playerId)
    {
        IReadOnlyList<PlayerIdentity> friends = _friendshipRepository.GetFriends(playerId);
        List<FriendPresence> friendPresences = friends
            .Select(friend => new FriendPresence(friend, _presenceUseCases.GetStatus(friend.PlayerId)))
            .ToList();

        return friendPresences;
    }

    public IReadOnlyList<FriendRequestSummary> GetPendingRequests(int playerId)
    {
        IReadOnlyList<FriendRequestSummary> pendingRequests = _friendshipRepository.GetPendingRequests(playerId);

        return pendingRequests;
    }

    public IReadOnlyList<PlayerIdentity> SearchPlayers(int playerId, string usernameFragment)
    {
        string normalizedFragment = PlayerInputValidator.NormalizeUsername(usernameFragment);
        IReadOnlyList<PlayerIdentity> players = _friendshipRepository.SearchRegisteredPlayers(
            playerId,
            normalizedFragment);

        return players;
    }

    public SendFriendRequestStatus SendFriendRequest(int requesterPlayerId, string addresseeUsername)
    {
        string normalizedUsername = PlayerInputValidator.NormalizeUsername(addresseeUsername);
        PlayerIdentity addressee = _friendshipRepository.FindRegisteredPlayer(normalizedUsername);
        SendFriendRequestStatus status = addressee.IsEmpty
            ? SendFriendRequestStatus.PlayerNotFound
            : EvaluateFriendRequest(requesterPlayerId, addressee.PlayerId);

        LogFriendRequest(requesterPlayerId, status);
        return status;
    }

    public bool AcceptFriendRequest(int playerId, int requestId)
    {
        IReadOnlyList<int> requesterPlayerIds = _friendshipRepository.AcceptRequest(requestId, playerId);
        bool isAccepted = requesterPlayerIds.Any();

        foreach (int requesterPlayerId in requesterPlayerIds)
        {
            _socialNotifier.NotifyFriendsChanged(requesterPlayerId);
        }

        LogRequestDecision(playerId, isAccepted);
        return isAccepted;
    }

    public bool RejectFriendRequest(int playerId, int requestId)
    {
        bool isRejected = _friendshipRepository.RejectRequest(requestId, playerId);

        LogRequestDecision(playerId, isRejected);
        return isRejected;
    }

    public RemoveFriendStatus RemoveFriend(int playerId, string friendUsername)
    {
        string normalizedUsername = PlayerInputValidator.NormalizeUsername(friendUsername);
        PlayerIdentity friend = _friendshipRepository.FindRegisteredPlayer(normalizedUsername);
        RemoveFriendStatus status = friend.IsEmpty
            ? RemoveFriendStatus.PlayerNotFound
            : RemoveExistingFriend(playerId, friend.PlayerId);

        LogFriendRemoval(playerId, status);
        return status;
    }

    public void PublishStatus(int playerId)
    {
        try
        {
            IReadOnlyList<PlayerIdentity> friends = _friendshipRepository.GetFriends(playerId);
            List<int> friendPlayerIds = friends.Select(friend => friend.PlayerId).ToList();
            PlayerStatus status = _presenceUseCases.GetStatus(playerId);

            _socialNotifier.NotifyFriendStatusChanged(new FriendStatusNotice(friendPlayerIds, playerId, status));
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("A status change was not published because the database is unavailable.", exception);
        }
    }

    private static SendFriendRequestStatus ToRequestStatus(FriendshipState state)
    {
        SendFriendRequestStatus status;

        switch (state)
        {
            case FriendshipState.Accepted:
                status = SendFriendRequestStatus.AlreadyFriends;
                break;
            case FriendshipState.Pending:
                status = SendFriendRequestStatus.RequestAlreadyExists;
                break;
            case FriendshipState.Blocked:
                status = SendFriendRequestStatus.PlayerBlocked;
                break;
            default:
                status = SendFriendRequestStatus.Success;
                break;
        }

        return status;
    }

    private static void LogFriendRequest(int requesterPlayerId, SendFriendRequestStatus status)
    {
        if (status == SendFriendRequestStatus.Success)
        {
            _logger.InfoFormat("A friend request was sent. PlayerId: {0}.", requesterPlayerId);
        }
        else
        {
            _logger.WarnFormat(
                "A friend request was rejected. PlayerId: {0}. Status: {1}.",
                requesterPlayerId,
                status);
        }
    }

    private static void LogRequestDecision(int playerId, bool wasApplied)
    {
        if (wasApplied)
        {
            _logger.InfoFormat("A friend request was answered. PlayerId: {0}.", playerId);
        }
        else
        {
            _logger.WarnFormat(
                "A friend request answer was rejected because the request was not found. PlayerId: {0}.",
                playerId);
        }
    }

    private static void LogFriendRemoval(int playerId, RemoveFriendStatus status)
    {
        if (status == RemoveFriendStatus.Success)
        {
            _logger.InfoFormat("A friend was removed. PlayerId: {0}.", playerId);
        }
        else
        {
            _logger.WarnFormat("A friend removal was rejected. PlayerId: {0}. Status: {1}.", playerId, status);
        }
    }

    private SendFriendRequestStatus EvaluateFriendRequest(int requesterPlayerId, int addresseePlayerId)
    {
        SendFriendRequestStatus status = SendFriendRequestStatus.CannotTargetSelf;
        bool isDifferentPlayer = requesterPlayerId != addresseePlayerId;

        if (isDifferentPlayer)
        {
            FriendshipState state = _friendshipRepository.GetFriendshipState(requesterPlayerId, addresseePlayerId);
            status = ToRequestStatus(state);
        }

        bool canCreateRequest = status == SendFriendRequestStatus.Success;

        if (canCreateRequest)
        {
            bool isCreated = _friendshipRepository.CreateRequest(requesterPlayerId, addresseePlayerId);
            status = isCreated ? SendFriendRequestStatus.Success : SendFriendRequestStatus.RequestAlreadyExists;
        }

        if (status == SendFriendRequestStatus.Success)
        {
            _socialNotifier.NotifyFriendsChanged(addresseePlayerId);
        }

        return status;
    }

    private RemoveFriendStatus RemoveExistingFriend(int playerId, int friendPlayerId)
    {
        bool isRemoved = _friendshipRepository.RemoveFriendship(playerId, friendPlayerId);
        RemoveFriendStatus status = isRemoved ? RemoveFriendStatus.Success : RemoveFriendStatus.NotFriends;

        if (isRemoved)
        {
            _socialNotifier.NotifyFriendsChanged(friendPlayerId);
        }

        return status;
    }
}
