using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Persistence.Data.Repositories;

public sealed class FriendshipRepository : IFriendshipRepository
{
    private const byte RegisteredPlayerFlag = 0;
    private const int PendingStatusId = 1;
    private const int AcceptedStatusId = 2;
    private const int RejectedStatusId = 3;
    private const int BlockedStatusId = 4;
    private const int MaximumSearchResults = 10;

    private readonly DatabaseExecutor _databaseExecutor;

    public FriendshipRepository(DatabaseExecutor databaseExecutor)
    {
        _databaseExecutor = databaseExecutor ?? throw new ArgumentNullException(nameof(databaseExecutor));
    }

    public bool TryFindPlayerId(string username, out int playerId)
    {
        string normalizedUsername = username ?? string.Empty;
        List<int> playerIds = _databaseExecutor.Execute(
            context => FindRegisteredPlayerIds(context, normalizedUsername));
        bool isFound = playerIds.Any();
        playerId = playerIds.FirstOrDefault();

        return isFound;
    }

    public IReadOnlyList<PlayerIdentity> GetFriends(int playerId)
    {
        List<PlayerIdentity> friends = _databaseExecutor.Execute(
            context => FindFriends(context, playerId));
        return friends;
    }

    public IReadOnlyList<FriendRequestSummary> GetPendingRequests(int playerId)
    {
        List<FriendRequestSummary> friendRequests = _databaseExecutor.Execute(
            context => FindPendingRequests(context, playerId));
        return friendRequests;
    }

    public IReadOnlyList<PlayerIdentity> SearchRegisteredPlayers(int playerId, string usernameFragment)
    {
        string normalizedFragment = usernameFragment ?? string.Empty;
        List<PlayerIdentity> players = _databaseExecutor.Execute(
            context => FindPlayersByUsername(context, playerId, normalizedFragment));
        return players;
    }

    public FriendshipState GetFriendshipState(int playerId, int friendPlayerId)
    {
        List<int> statusIds = _databaseExecutor.Execute(
            context => FindPairStatusIds(context, playerId, friendPlayerId));
        FriendshipState friendshipState = ToFriendshipState(statusIds.FirstOrDefault());

        return friendshipState;
    }

    public bool CreateRequest(int requesterPlayerId, int addresseePlayerId)
    {
        bool isCreated = _databaseExecutor.Execute(
            context => InsertOrReopenRequest(context, requesterPlayerId, addresseePlayerId));

        return isCreated;
    }

    public bool TryAcceptRequest(int requestId, int addresseePlayerId, out int requesterPlayerId)
    {
        List<int> requesterPlayerIds = _databaseExecutor.Execute(
            context => ChangePendingStatus(context, IsRequestFor(requestId, addresseePlayerId), AcceptedStatusId));
        bool isAccepted = requesterPlayerIds.Any();

        requesterPlayerId = requesterPlayerIds.FirstOrDefault();
        return isAccepted;
    }

    public bool RejectRequest(int requestId, int addresseePlayerId)
    {
        List<int> requesterPlayerIds = _databaseExecutor.Execute(
            context => ChangePendingStatus(context, IsRequestFor(requestId, addresseePlayerId), RejectedStatusId));
        bool isRejected = requesterPlayerIds.Any();

        return isRejected;
    }

    public bool RemoveFriendship(int playerId, int friendPlayerId)
    {
        bool isRemoved = _databaseExecutor.Execute(
            context => DeleteAcceptedFriendship(context, playerId, friendPlayerId));

        return isRemoved;
    }

    private static List<int> FindRegisteredPlayerIds(DominoGameDBEntities context, string username)
    {
        List<int> playerIds = context.Players
            .AsNoTracking()
            .Where(player => player.IsGuest == RegisteredPlayerFlag && player.Username == username)
            .Select(player => player.PlayerId)
            .ToList();
        return playerIds;
    }

    private static List<PlayerIdentity> FindPlayersByUsername(
        DominoGameDBEntities context,
        int playerId,
        string usernameFragment)
    {
        List<PlayerIdentity> players = context.Players
            .AsNoTracking()
            .Where(player => player.IsGuest == RegisteredPlayerFlag && player.PlayerId != playerId)
            .Where(player => player.Username.Contains(usernameFragment))
            .OrderBy(player => player.Username)
            .Take(MaximumSearchResults)
            .Select(player => new { player.PlayerId, player.Username })
            .AsEnumerable()
            .Select(row => new PlayerIdentity(row.PlayerId, row.Username))
            .ToList();
        return players;
    }

    private static List<PlayerIdentity> FindFriends(DominoGameDBEntities context, int playerId)
    {
        IQueryable<FriendshipRequest> acceptedRequests = context.FriendshipRequests
            .AsNoTracking()
            .Where(request => request.IdStatus == AcceptedStatusId);

        IQueryable<Player> friendsAddedByPlayer = acceptedRequests
            .Where(request => request.RequesterPlayerId == playerId)
            .Select(request => request.Player);
        IQueryable<Player> friendsWhoAddedPlayer = acceptedRequests
            .Where(request => request.AddresseePlayerId == playerId)
            .Select(request => request.Player1);

        List<PlayerIdentity> friends = friendsAddedByPlayer
            .Concat(friendsWhoAddedPlayer)
            .OrderBy(friend => friend.Username)
            .Select(friend => new { friend.PlayerId, friend.Username })
            .AsEnumerable()
            .Select(row => new PlayerIdentity(row.PlayerId, row.Username))
            .ToList();
        return friends;
    }

    private static List<FriendRequestSummary> FindPendingRequests(DominoGameDBEntities context, int playerId)
    {
        List<FriendRequestSummary> requests = context.FriendshipRequests
            .AsNoTracking()
            .Where(request => request.AddresseePlayerId == playerId && request.IdStatus == PendingStatusId)
            .OrderBy(request => request.RequestedAt)
            .Select(request => new { request.FriendshipRequestsID, request.Player1.Username, request.RequestedAt })
            .AsEnumerable()
            .Select(row => new FriendRequestSummary(row.FriendshipRequestsID, row.Username, row.RequestedAt))
            .ToList();
        return requests;
    }

    private static List<int> FindPairStatusIds(DominoGameDBEntities context, int playerId, int otherPlayerId)
    {
        List<int> statusIds = FindPair(context, playerId, otherPlayerId)
            .AsNoTracking()
            .Select(request => request.IdStatus)
            .ToList();
        return statusIds;
    }

    private static IQueryable<FriendshipRequest> FindPair(
        DominoGameDBEntities context,
        int playerId,
        int otherPlayerId)
    {
        IQueryable<FriendshipRequest> sentRequests = context.FriendshipRequests
            .Where(request => request.RequesterPlayerId == playerId && request.AddresseePlayerId == otherPlayerId);
        IQueryable<FriendshipRequest> receivedRequests = context.FriendshipRequests
            .Where(request => request.RequesterPlayerId == otherPlayerId && request.AddresseePlayerId == playerId);

        IQueryable<FriendshipRequest> pair = sentRequests.Concat(receivedRequests);
        return pair;
    }

    private static bool InsertOrReopenRequest(
        DominoGameDBEntities context,
        int requesterPlayerId,
        int addresseePlayerId)
    {
        bool isCreated;

        using (DbContextTransaction transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
        {
            List<FriendshipRequest> existingRequests = FindPair(context, requesterPlayerId, addresseePlayerId)
                .ToList();
            bool hasExistingRequest = existingRequests.Any();

            if (hasExistingRequest)
            {
                isCreated = ReopenRejectedRequest(
                    existingRequests.First(),
                    requesterPlayerId,
                    addresseePlayerId);
            }
            else
            {
                context.FriendshipRequests.Add(CreatePendingRequest(requesterPlayerId, addresseePlayerId));
                isCreated = true;
            }

            context.SaveChanges();
            transaction.Commit();
        }

        return isCreated;
    }

    private static bool ReopenRejectedRequest(
        FriendshipRequest request,
        int requesterPlayerId,
        int addresseePlayerId)
    {
        bool isRejected = request.IdStatus == RejectedStatusId;

        if (isRejected)
        {
            request.RequesterPlayerId = requesterPlayerId;
            request.AddresseePlayerId = addresseePlayerId;
            request.IdStatus = PendingStatusId;
            request.RequestedAt = DateTime.UtcNow;
        }

        return isRejected;
    }

    private static FriendshipRequest CreatePendingRequest(int requesterPlayerId, int addresseePlayerId)
    {
        FriendshipRequest request = new FriendshipRequest
        {
            RequesterPlayerId = requesterPlayerId,
            AddresseePlayerId = addresseePlayerId,
            IdStatus = PendingStatusId,
            RequestedAt = DateTime.UtcNow
        };
        return request;
    }

    private static Expression<Func<FriendshipRequest, bool>> IsRequestFor(int requestId, int addresseePlayerId)
    {
        Expression<Func<FriendshipRequest, bool>> predicate =
            request => request.FriendshipRequestsID == requestId && request.AddresseePlayerId == addresseePlayerId;
        return predicate;
    }

    private static List<int> ChangePendingStatus(
        DominoGameDBEntities context,
        Expression<Func<FriendshipRequest, bool>> requestFilter,
        int newStatusId)
    {
        List<FriendshipRequest> pendingRequests = context.FriendshipRequests
            .Where(requestFilter)
            .Where(request => request.IdStatus == PendingStatusId)
            .ToList();

        foreach (FriendshipRequest pendingRequest in pendingRequests)
        {
            pendingRequest.IdStatus = newStatusId;
        }

        context.SaveChanges();

        List<int> requesterPlayerIds = pendingRequests
            .Select(request => request.RequesterPlayerId)
            .ToList();
        return requesterPlayerIds;
    }

    private static bool DeleteAcceptedFriendship(DominoGameDBEntities context, int playerId, int friendPlayerId)
    {
        List<FriendshipRequest> friendships = FindPair(context, playerId, friendPlayerId)
            .Where(request => request.IdStatus == AcceptedStatusId)
            .ToList();
        bool isFound = friendships.Any();

        if (isFound)
        {
            context.FriendshipRequests.RemoveRange(friendships);
            context.SaveChanges();
        }

        return isFound;
    }

    private static FriendshipState ToFriendshipState(int statusId)
    {
        FriendshipState state;

        switch (statusId)
        {
            case PendingStatusId:
                state = FriendshipState.Pending;
                break;
            case AcceptedStatusId:
                state = FriendshipState.Accepted;
                break;
            case RejectedStatusId:
                state = FriendshipState.Rejected;
                break;
            case BlockedStatusId:
                state = FriendshipState.Blocked;
                break;
            default:
                state = FriendshipState.None;
                break;
        }

        return state;
    }
}
