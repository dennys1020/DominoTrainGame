using System;
using System.Collections.Generic;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class FriendUseCases : IFriendUseCases
{
    private const int MaximumUsernameLength = 45;
    private static readonly ITraceLog _logger;
    private readonly IFriendshipRepository _friendshipRepository;

    static FriendUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(FriendUseCases));
    }

    public FriendUseCases(IFriendshipRepository friendshipRepository)
    {
        _friendshipRepository = friendshipRepository ?? throw new
            ArgumentNullException(nameof(friendshipRepository));
    }

    public IReadOnlyList<FriendSummary> GetFriends(int playerId)
    {
        IReadOnlyList<FriendSummary> friend = _friendshipRepository.GetFriends(playerId);

        return friend;
    }

    public IReadOnlyList<FriendRequestSummary> GetPendingRequests(int playerId)
    {
        IReadOnlyList<FriendRequestSummary> pendingRequests = _friendshipRepository.GetPendingRequests(playerId);

        return pendingRequests;
    }

    public IReadOnlyList<FriendSummary> SearchPlayers(int playerId, string usernameFragment)
    {
        string normalizedFragment = NormalizeUsername(usernameFragment);
        IReadOnlyList<FriendSummary> players = _friendshipRepository.SearchRegisteredPlayers(
            playerId,
            normalizedFragment);

        return players;
    }

    public SendFriendRequestStatus SendFriendRequest(int requesterPlayerId, string addresseeUsername)
    {
        string normalizedUsername = NormalizeUsername(addresseeUsername);
        bool isFound = _friendshipRepository.TryFindFriendship(normalizedUsername, out int addresseePlayerId);
        SendFriendRequestStatus status = isFound
            ? EvaluateFriendRequest(requesterPlayerId, addresseePlayerId)
            : SendFriendRequestStatus.PlayerNotFound;

        LogFriendRequest(requesterPlayerId, status);
        return status;
    }

    public bool AcceptFriendRequest(int playerId, int requestId)
    {
        bool isAccepted = _friendshipRepository.AcceptRequest(requestId, playerId);

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
        string normalizedUsername = NormalizeUsername(friendUsername);
        bool isFound = _friendshipRepository.TryFindFriendship(normalizedUsername, out int friendPlayerId);
        RemoveFriendStatus status = isFound
            ? RemoveExistingFriend(playerId, friendPlayerId)
            : RemoveFriendStatus.PlayerNotFound;

        LogFriendRemoval(playerId, status);
        return status;
    }

    private static string NormalizeUsername(string username)
    {
        string normalizedUsername = username?.Trim() ?? string.Empty;
        bool isUsernameValid = !string.IsNullOrWhiteSpace(normalizedUsername)
            && normalizedUsername.Length <= MaximumUsernameLength;

        if (!isUsernameValid)
        {
            throw new ArgumentException(
                "The username is empty or exceeds the maximum allowed length.",
                nameof(username));
        }

        return normalizedUsername;
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

        return status;
    }

    private RemoveFriendStatus RemoveExistingFriend(int playerId, int friendPlayerId)
    {
        bool isRemoved = _friendshipRepository.RemoveFriendship(playerId, friendPlayerId);
        RemoveFriendStatus status = isRemoved ? RemoveFriendStatus.Success : RemoveFriendStatus.NotFriends;
        return status;
    }
}
