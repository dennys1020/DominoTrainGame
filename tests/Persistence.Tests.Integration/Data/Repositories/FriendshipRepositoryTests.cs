using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Persistence.Data;
using DominoTrainGame.Server.Persistence.Data.Repositories;
using NUnit.Framework;

namespace DominoTrainGame.Server.Persistence.Tests.Integration.Data.Repositories;

[TestFixture]
public sealed class FriendshipRepositoryTests
{
    private const int UnknownRequestId = -1;
    private const string RequesterUsername = "samuel";
    private const string RequesterEmail = "samuel@uv.mx";
    private const string AddresseeUsername = "denisse";
    private const string AddresseeEmail = "denisse@uv.mx";
    private const string GuestUsername = "Guestsam01";
    private const string RequesterUsernameFragment = "sam";
    private const string UnknownUsername = "nobody";
    private const string PasswordHash = "stored-hash";

    private TransactionScope _transaction;
    private DatabaseExecutor _databaseExecutor;
    private PlayerRepository _playerRepository;
    private FriendshipRepository _friendshipRepository;
    private int _requesterPlayerId;
    private int _addresseePlayerId;

    [SetUp]
    public void SetUp()
    {
        _transaction = new TransactionScope();
        _databaseExecutor = new DatabaseExecutor();
        _playerRepository = new PlayerRepository(_databaseExecutor);
        _friendshipRepository = new FriendshipRepository(_databaseExecutor);

        _requesterPlayerId = CreateRegisteredPlayer(RequesterUsername, RequesterEmail);
        _addresseePlayerId = CreateRegisteredPlayer(AddresseeUsername, AddresseeEmail);
    }

    [TearDown]
    public void TearDown()
    {
        _transaction.Dispose();
    }

    [Test]
    public void Test_Constructor_NullDatabaseExecutor_ThrowsArgumentNullException()
    {
        _databaseExecutor = null;

        Assert.Throws<ArgumentNullException>(CreateFriendshipRepository);
    }

    [Test]
    public void Test_FindRegisteredPlayer_RegisteredUsername_ReturnsPlayerId()
    {
        PlayerIdentity player = _friendshipRepository.FindRegisteredPlayer(AddresseeUsername);

        Assert.That(player.PlayerId, Is.EqualTo(_addresseePlayerId));
    }

    [Test]
    public void Test_FindRegisteredPlayer_GuestUsername_ReturnsEmptyIdentity()
    {
        _playerRepository.CreateGuest(GuestUsername);

        PlayerIdentity player = _friendshipRepository.FindRegisteredPlayer(GuestUsername);

        Assert.That(player.IsEmpty, Is.True);
    }

    [Test]
    public void Test_FindRegisteredPlayer_UnknownUsername_ReturnsEmptyIdentity()
    {
        PlayerIdentity player = _friendshipRepository.FindRegisteredPlayer(UnknownUsername);

        Assert.That(player.IsEmpty, Is.True);
    }

    [Test]
    public void Test_SearchRegisteredPlayers_MatchingFragment_ReturnsMatchingPlayer()
    {
        IReadOnlyList<PlayerIdentity> players = _friendshipRepository.SearchRegisteredPlayers(
            _addresseePlayerId,
            RequesterUsernameFragment);

        Assert.That(players.Single().PlayerId, Is.EqualTo(_requesterPlayerId));
    }

    [Test]
    public void Test_SearchRegisteredPlayers_FragmentOfOwnUsername_ExcludesSearcher()
    {
        IReadOnlyList<PlayerIdentity> players = _friendshipRepository.SearchRegisteredPlayers(
            _requesterPlayerId,
            RequesterUsernameFragment);

        Assert.That(players, Is.Empty);
    }

    [Test]
    public void Test_SearchRegisteredPlayers_GuestMatchesFragment_ExcludesGuest()
    {
        _playerRepository.CreateGuest(GuestUsername);

        IReadOnlyList<PlayerIdentity> players = _friendshipRepository.SearchRegisteredPlayers(
            _addresseePlayerId,
            RequesterUsernameFragment);

        Assert.That(players.Single().PlayerId, Is.EqualTo(_requesterPlayerId));
    }

    [Test]
    public void Test_GetFriendshipState_NoRequest_ReturnsNone()
    {
        FriendshipState state = _friendshipRepository.GetFriendshipState(_requesterPlayerId, _addresseePlayerId);

        Assert.That(state, Is.EqualTo(FriendshipState.None));
    }

    [Test]
    public void Test_CreateRequest_NoPreviousRequest_ReturnsTrue()
    {
        bool isCreated = _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        Assert.That(isCreated, Is.True);
    }

    [Test]
    public void Test_CreateRequest_NoPreviousRequest_LeavesRequestPending()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        FriendshipState state = _friendshipRepository.GetFriendshipState(_addresseePlayerId, _requesterPlayerId);

        Assert.That(state, Is.EqualTo(FriendshipState.Pending));
    }

    [Test]
    public void Test_CreateRequest_PendingRequestExists_ReturnsFalse()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        bool isCreated = _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        Assert.That(isCreated, Is.False);
    }

    [Test]
    public void Test_CreateRequest_ReversePendingRequestExists_ReturnsFalse()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        bool isCreated = _friendshipRepository.CreateRequest(_addresseePlayerId, _requesterPlayerId);

        Assert.That(isCreated, Is.False);
    }

    [Test]
    public void Test_CreateRequest_RejectedRequestExists_ReopensRequest()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);
        _friendshipRepository.RejectRequest(FindPendingRequestId(), _addresseePlayerId);

        bool isCreated = _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        Assert.That(isCreated, Is.True);
    }

    [Test]
    public void Test_GetPendingRequests_RequestReceived_ReturnsRequesterUsername()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        IReadOnlyList<PendingFriendshipSummary> requests = _friendshipRepository.GetPendingRequests(
            _addresseePlayerId);

        Assert.That(requests.Single().RequesterUsername, Is.EqualTo(RequesterUsername));
    }

    [Test]
    public void Test_GetPendingRequests_RequestSent_ReturnsNoRequests()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        IReadOnlyList<PendingFriendshipSummary> requests = _friendshipRepository.GetPendingRequests(
            _requesterPlayerId);

        Assert.That(requests, Is.Empty);
    }

    [Test]
    public void Test_AcceptRequest_PendingRequest_ReturnsRequesterPlayerId()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        IReadOnlyList<int> requesterPlayerIds = _friendshipRepository.AcceptRequest(
            FindPendingRequestId(),
            _addresseePlayerId);

        Assert.That(requesterPlayerIds, Is.EqualTo(new[] { _requesterPlayerId }));
    }

    [Test]
    public void Test_AcceptRequest_PendingRequest_MakesAddresseeAFriend()
    {
        MakeArrangedPlayersFriends();

        IReadOnlyList<PlayerIdentity> friends = _friendshipRepository.GetFriends(_requesterPlayerId);

        Assert.That(friends.Single().PlayerId, Is.EqualTo(_addresseePlayerId));
    }

    [Test]
    public void Test_AcceptRequest_PendingRequest_MakesRequesterAFriend()
    {
        MakeArrangedPlayersFriends();

        IReadOnlyList<PlayerIdentity> friends = _friendshipRepository.GetFriends(_addresseePlayerId);

        Assert.That(friends.Single().PlayerId, Is.EqualTo(_requesterPlayerId));
    }

    [Test]
    public void Test_AcceptRequest_RequestOfOtherAddressee_ReturnsNoRequesters()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        IReadOnlyList<int> requesterPlayerIds = _friendshipRepository.AcceptRequest(
            FindPendingRequestId(),
            _requesterPlayerId);

        Assert.That(requesterPlayerIds, Is.Empty);
    }

    [Test]
    public void Test_RejectRequest_PendingRequest_ReturnsTrue()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        bool isRejected = _friendshipRepository.RejectRequest(FindPendingRequestId(), _addresseePlayerId);

        Assert.That(isRejected, Is.True);
    }

    [Test]
    public void Test_RejectRequest_PendingRequest_MarksRequestRejected()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);
        _friendshipRepository.RejectRequest(FindPendingRequestId(), _addresseePlayerId);

        FriendshipState state = _friendshipRepository.GetFriendshipState(_requesterPlayerId, _addresseePlayerId);

        Assert.That(state, Is.EqualTo(FriendshipState.Rejected));
    }

    [Test]
    public void Test_RejectRequest_UnknownRequest_ReturnsFalse()
    {
        bool isRejected = _friendshipRepository.RejectRequest(UnknownRequestId, _addresseePlayerId);

        Assert.That(isRejected, Is.False);
    }

    [Test]
    public void Test_RemoveFriendship_Friends_ReturnsTrue()
    {
        MakeArrangedPlayersFriends();

        bool isRemoved = _friendshipRepository.RemoveFriendship(_addresseePlayerId, _requesterPlayerId);

        Assert.That(isRemoved, Is.True);
    }

    [Test]
    public void Test_RemoveFriendship_Friends_RemovesFriend()
    {
        MakeArrangedPlayersFriends();
        _friendshipRepository.RemoveFriendship(_addresseePlayerId, _requesterPlayerId);

        IReadOnlyList<PlayerIdentity> friends = _friendshipRepository.GetFriends(_requesterPlayerId);

        Assert.That(friends, Is.Empty);
    }

    [Test]
    public void Test_RemoveFriendship_PendingRequest_ReturnsFalse()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);

        bool isRemoved = _friendshipRepository.RemoveFriendship(_requesterPlayerId, _addresseePlayerId);

        Assert.That(isRemoved, Is.False);
    }

    private int CreateRegisteredPlayer(string username, string email)
    {
        _playerRepository.CreatePlayer(new PlayerRegistration(username, email, PasswordHash));
        int playerId = _playerRepository.FindCredentials(username).PlayerId;
        return playerId;
    }

    private void CreateFriendshipRepository()
    {
        _friendshipRepository = new FriendshipRepository(_databaseExecutor);
    }

    private int FindPendingRequestId()
    {
        int requestId = _friendshipRepository.GetPendingRequests(_addresseePlayerId).Single().RequestId;
        return requestId;
    }

    private void MakeArrangedPlayersFriends()
    {
        _friendshipRepository.CreateRequest(_requesterPlayerId, _addresseePlayerId);
        _friendshipRepository.AcceptRequest(FindPendingRequestId(), _addresseePlayerId);
    }
}
