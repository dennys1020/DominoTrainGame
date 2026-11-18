using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.UseCases;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace DominoTrainGame.Server.Application.Tests.Unit.UseCases;

[TestFixture]
public sealed class FriendUseCasesTests
{
    private const int PlayerId = 7;
    private const int FriendPlayerId = 12;
    private const int RequestId = 30;
    private const int UnknownRequestId = 99;
    private const int ExpectedCallCount = 1;
    private const int NoticeArgumentIndex = 0;
    private const int MaximumUsernameLength = 45;
    private const int TooLongUsernameLength = MaximumUsernameLength + 1;
    private const char FillerCharacter = 'a';
    private const string PlayerUsername = "samuel";
    private const string FriendUsername = "denisse";
    private const string PaddedFriendUsername = "  denisse  ";
    private const string RequesterUsername = "carlos";
    private const string UnknownUsername = "nobody";
    private const string UsernameFragment = "den";
    private const string PaddedUsernameFragment = "  den  ";
    private const string DatabaseErrorMessage = "The database is unavailable.";

    private IFriendshipRepository _friendshipRepository;
    private IPresenceUseCases _presenceUseCases;
    private ISocialNotifier _socialNotifier;
    private FriendUseCases _friendUseCases;
    private List<PlayerIdentity> _friends;
    private List<PendingFriendshipSummary> _pendingRequests;
    private string _usernameFragment;
    private string _friendUsername;

    [SetUp]
    public void SetUp()
    {
        _friendshipRepository = Substitute.For<IFriendshipRepository>();
        _presenceUseCases = Substitute.For<IPresenceUseCases>();
        _socialNotifier = Substitute.For<ISocialNotifier>();
        _friends = new List<PlayerIdentity> { new PlayerIdentity(FriendPlayerId, FriendUsername) };
        _pendingRequests = new List<PendingFriendshipSummary>
        {
            new PendingFriendshipSummary(RequestId, RequesterUsername, DateTime.MinValue)
        };

        _friendshipRepository.GetFriends(PlayerId).Returns(_friends);
        _friendshipRepository.GetPendingRequests(PlayerId).Returns(_pendingRequests);
        _friendshipRepository.SearchRegisteredPlayers(PlayerId, UsernameFragment).Returns(_friends);
        _friendshipRepository.FindRegisteredPlayer(Arg.Any<string>()).Returns(PlayerIdentity.Empty);
        _friendshipRepository.FindRegisteredPlayer(PlayerUsername)
            .Returns(new PlayerIdentity(PlayerId, PlayerUsername));
        _friendshipRepository.FindRegisteredPlayer(FriendUsername)
            .Returns(new PlayerIdentity(FriendPlayerId, FriendUsername));
        _friendshipRepository.GetFriendshipState(PlayerId, FriendPlayerId).Returns(FriendshipState.None);
        _friendshipRepository.CreateRequest(PlayerId, FriendPlayerId).Returns(true);
        _friendshipRepository.AcceptRequest(Arg.Any<int>(), Arg.Any<int>()).Returns(Array.Empty<int>());
        _friendshipRepository.AcceptRequest(RequestId, FriendPlayerId).Returns(new[] { PlayerId });
        _friendshipRepository.RejectRequest(RequestId, FriendPlayerId).Returns(true);
        _friendshipRepository.RemoveFriendship(PlayerId, FriendPlayerId).Returns(true);
        _presenceUseCases.GetStatus(FriendPlayerId).Returns(PlayerStatus.InMatch);
        _presenceUseCases.GetStatus(PlayerId).Returns(PlayerStatus.Online);

        _usernameFragment = UsernameFragment;
        _friendUsername = FriendUsername;
        CreateFriendUseCases();
    }

    [Test]
    public void Test_Constructor_NullFriendshipRepository_ThrowsArgumentNullException()
    {
        _friendshipRepository = null;

        Assert.Throws<ArgumentNullException>(CreateFriendUseCases);
    }

    [Test]
    public void Test_Constructor_NullPresenceUseCases_ThrowsArgumentNullException()
    {
        _presenceUseCases = null;

        Assert.Throws<ArgumentNullException>(CreateFriendUseCases);
    }

    [Test]
    public void Test_Constructor_NullSocialNotifier_ThrowsArgumentNullException()
    {
        _socialNotifier = null;

        Assert.Throws<ArgumentNullException>(CreateFriendUseCases);
    }

    [Test]
    public void Test_GetFriends_PlayerHasFriend_ReturnsFriendPlayerId()
    {
        IReadOnlyList<FriendPresence> friends = _friendUseCases.GetFriends(PlayerId);

        Assert.That(friends.Single().PlayerId, Is.EqualTo(FriendPlayerId));
    }

    [Test]
    public void Test_GetFriends_PlayerHasFriend_ReturnsFriendPresenceStatus()
    {
        IReadOnlyList<FriendPresence> friends = _friendUseCases.GetFriends(PlayerId);

        Assert.That(friends.Single().Status, Is.EqualTo(PlayerStatus.InMatch));
    }

    [Test]
    public void Test_GetFriends_PlayerHasNoFriends_ReturnsEmptyList()
    {
        _friendshipRepository.GetFriends(PlayerId).Returns(new List<PlayerIdentity>());

        IReadOnlyList<FriendPresence> friends = _friendUseCases.GetFriends(PlayerId);

        Assert.That(friends, Is.Empty);
    }

    [Test]
    public void Test_GetFriends_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _friendshipRepository.GetFriends(PlayerId).Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(GetPlayerFriends);
    }

    [Test]
    public void Test_GetPendingRequests_PlayerHasRequests_ReturnsRepositoryRequests()
    {
        IReadOnlyList<PendingFriendshipSummary> pendingRequests = _friendUseCases.GetPendingRequests(PlayerId);

        Assert.That(pendingRequests, Is.SameAs(_pendingRequests));
    }

    [Test]
    public void Test_GetPendingRequests_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _friendshipRepository.GetPendingRequests(PlayerId)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(GetPlayerPendingRequests);
    }

    [Test]
    public void Test_SearchPlayers_ValidFragment_ReturnsRepositoryPlayers()
    {
        IReadOnlyList<PlayerIdentity> players = _friendUseCases.SearchPlayers(PlayerId, UsernameFragment);

        Assert.That(players, Is.SameAs(_friends));
    }

    [Test]
    public void Test_SearchPlayers_PaddedFragment_SearchesTrimmedFragment()
    {
        _friendUseCases.SearchPlayers(PlayerId, PaddedUsernameFragment);

        _friendshipRepository.Received(ExpectedCallCount).SearchRegisteredPlayers(PlayerId, UsernameFragment);
    }

    [Test]
    public void Test_SearchPlayers_NullFragment_ThrowsArgumentException()
    {
        _usernameFragment = null;

        Assert.Throws<ArgumentException>(SearchPlayersWithArrangedFragment);
    }

    [Test]
    public void Test_SearchPlayers_TooLongFragment_ThrowsArgumentException()
    {
        _usernameFragment = CreateText(TooLongUsernameLength);

        Assert.Throws<ArgumentException>(SearchPlayersWithArrangedFragment);
    }

    [Test]
    public void Test_SendFriendRequest_NewAddressee_ReturnsSuccess()
    {
        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(PlayerId, FriendUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.Success));
    }

    [Test]
    public void Test_SendFriendRequest_NewAddressee_CreatesRequest()
    {
        _friendUseCases.SendFriendRequest(PlayerId, FriendUsername);

        _friendshipRepository.Received(ExpectedCallCount).CreateRequest(PlayerId, FriendPlayerId);
    }

    [Test]
    public void Test_SendFriendRequest_NewAddressee_NotifiesAddressee()
    {
        _friendUseCases.SendFriendRequest(PlayerId, FriendUsername);

        _socialNotifier.Received(ExpectedCallCount).NotifyFriendsChanged(FriendPlayerId);
    }

    [Test]
    public void Test_SendFriendRequest_PaddedUsername_SearchesTrimmedUsername()
    {
        _friendUseCases.SendFriendRequest(PlayerId, PaddedFriendUsername);

        _friendshipRepository.Received(ExpectedCallCount).FindRegisteredPlayer(FriendUsername);
    }

    [Test]
    public void Test_SendFriendRequest_UnknownPlayer_ReturnsPlayerNotFound()
    {
        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(PlayerId, UnknownUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.PlayerNotFound));
    }

    [Test]
    public void Test_SendFriendRequest_UnknownPlayer_DoesNotCreateRequest()
    {
        _friendUseCases.SendFriendRequest(PlayerId, UnknownUsername);

        _friendshipRepository.DidNotReceive().CreateRequest(Arg.Any<int>(), Arg.Any<int>());
    }

    [Test]
    public void Test_SendFriendRequest_OwnUsername_ReturnsCannotTargetSelf()
    {
        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(PlayerId, PlayerUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.CannotTargetSelf));
    }

    [Test]
    public void Test_SendFriendRequest_AlreadyFriends_ReturnsAlreadyFriends()
    {
        _friendshipRepository.GetFriendshipState(PlayerId, FriendPlayerId)
            .Returns(FriendshipState.Accepted);

        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(PlayerId, FriendUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.AlreadyFriends));
    }

    [Test]
    public void Test_SendFriendRequest_AlreadyFriends_DoesNotNotify()
    {
        _friendshipRepository.GetFriendshipState(PlayerId, FriendPlayerId)
            .Returns(FriendshipState.Accepted);

        _friendUseCases.SendFriendRequest(PlayerId, FriendUsername);

        _socialNotifier.DidNotReceive().NotifyFriendsChanged(Arg.Any<int>());
    }

    [Test]
    public void Test_SendFriendRequest_PendingRequest_ReturnsRequestAlreadyExists()
    {
        _friendshipRepository.GetFriendshipState(PlayerId, FriendPlayerId)
            .Returns(FriendshipState.Pending);

        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(PlayerId, FriendUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.RequestAlreadyExists));
    }

    [Test]
    public void Test_SendFriendRequest_AddresseeBlocked_ReturnsPlayerBlocked()
    {
        _friendshipRepository.GetFriendshipState(PlayerId, FriendPlayerId)
            .Returns(FriendshipState.Blocked);

        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(PlayerId, FriendUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.PlayerBlocked));
    }

    [Test]
    public void Test_SendFriendRequest_PreviouslyRejected_ReturnsSuccess()
    {
        _friendshipRepository.GetFriendshipState(PlayerId, FriendPlayerId)
            .Returns(FriendshipState.Rejected);

        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(PlayerId, FriendUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.Success));
    }

    [Test]
    public void Test_SendFriendRequest_RequestNotCreated_ReturnsRequestAlreadyExists()
    {
        _friendshipRepository.CreateRequest(PlayerId, FriendPlayerId).Returns(false);

        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(PlayerId, FriendUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.RequestAlreadyExists));
    }

    [Test]
    public void Test_SendFriendRequest_NullUsername_ThrowsArgumentException()
    {
        _friendUsername = null;

        Assert.Throws<ArgumentException>(SendRequestToArrangedFriend);
    }

    [Test]
    public void Test_SendFriendRequest_TooLongUsername_ThrowsArgumentException()
    {
        _friendUsername = CreateText(TooLongUsernameLength);

        Assert.Throws<ArgumentException>(SendRequestToArrangedFriend);
    }

    [Test]
    public void Test_SendFriendRequest_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _friendshipRepository.FindRegisteredPlayer(FriendUsername)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(SendRequestToArrangedFriend);
    }

    [Test]
    public void Test_AcceptFriendRequest_ExistingRequest_ReturnsTrue()
    {
        bool isAccepted = _friendUseCases.AcceptFriendRequest(FriendPlayerId, RequestId);

        Assert.That(isAccepted, Is.True);
    }

    [Test]
    public void Test_AcceptFriendRequest_ExistingRequest_NotifiesRequester()
    {
        _friendUseCases.AcceptFriendRequest(FriendPlayerId, RequestId);

        _socialNotifier.Received(ExpectedCallCount).NotifyFriendsChanged(PlayerId);
    }

    [Test]
    public void Test_AcceptFriendRequest_RequestNotFound_ReturnsFalse()
    {
        bool isAccepted = _friendUseCases.AcceptFriendRequest(FriendPlayerId, UnknownRequestId);

        Assert.That(isAccepted, Is.False);
    }

    [Test]
    public void Test_AcceptFriendRequest_RequestNotFound_DoesNotNotify()
    {
        _friendUseCases.AcceptFriendRequest(FriendPlayerId, UnknownRequestId);

        _socialNotifier.DidNotReceive().NotifyFriendsChanged(Arg.Any<int>());
    }

    [Test]
    public void Test_AcceptFriendRequest_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _friendshipRepository.AcceptRequest(RequestId, FriendPlayerId)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(AcceptArrangedRequest);
    }

    [Test]
    public void Test_RejectFriendRequest_ExistingRequest_ReturnsTrue()
    {
        bool isRejected = _friendUseCases.RejectFriendRequest(FriendPlayerId, RequestId);

        Assert.That(isRejected, Is.True);
    }

    [Test]
    public void Test_RejectFriendRequest_RequestNotFound_ReturnsFalse()
    {
        bool isRejected = _friendUseCases.RejectFriendRequest(FriendPlayerId, UnknownRequestId);

        Assert.That(isRejected, Is.False);
    }

    [Test]
    public void Test_RejectFriendRequest_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _friendshipRepository.RejectRequest(RequestId, FriendPlayerId)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(RejectArrangedRequest);
    }

    [Test]
    public void Test_RemoveFriend_ExistingFriend_ReturnsSuccess()
    {
        RemoveFriendStatus status = _friendUseCases.RemoveFriend(PlayerId, FriendUsername);

        Assert.That(status, Is.EqualTo(RemoveFriendStatus.Success));
    }

    [Test]
    public void Test_RemoveFriend_ExistingFriend_RemovesFriendship()
    {
        _friendUseCases.RemoveFriend(PlayerId, FriendUsername);

        _friendshipRepository.Received(ExpectedCallCount).RemoveFriendship(PlayerId, FriendPlayerId);
    }

    [Test]
    public void Test_RemoveFriend_ExistingFriend_NotifiesFriend()
    {
        _friendUseCases.RemoveFriend(PlayerId, FriendUsername);

        _socialNotifier.Received(ExpectedCallCount).NotifyFriendsChanged(FriendPlayerId);
    }

    [Test]
    public void Test_RemoveFriend_PaddedUsername_SearchesTrimmedUsername()
    {
        _friendUseCases.RemoveFriend(PlayerId, PaddedFriendUsername);

        _friendshipRepository.Received(ExpectedCallCount).FindRegisteredPlayer(FriendUsername);
    }

    [Test]
    public void Test_RemoveFriend_UnknownPlayer_ReturnsPlayerNotFound()
    {
        RemoveFriendStatus status = _friendUseCases.RemoveFriend(PlayerId, UnknownUsername);

        Assert.That(status, Is.EqualTo(RemoveFriendStatus.PlayerNotFound));
    }

    [Test]
    public void Test_RemoveFriend_UnknownPlayer_DoesNotRemoveFriendship()
    {
        _friendUseCases.RemoveFriend(PlayerId, UnknownUsername);

        _friendshipRepository.DidNotReceive().RemoveFriendship(Arg.Any<int>(), Arg.Any<int>());
    }

    [Test]
    public void Test_RemoveFriend_NotFriends_ReturnsNotFriends()
    {
        _friendshipRepository.RemoveFriendship(PlayerId, FriendPlayerId).Returns(false);

        RemoveFriendStatus status = _friendUseCases.RemoveFriend(PlayerId, FriendUsername);

        Assert.That(status, Is.EqualTo(RemoveFriendStatus.NotFriends));
    }

    [Test]
    public void Test_RemoveFriend_NotFriends_DoesNotNotify()
    {
        _friendshipRepository.RemoveFriendship(PlayerId, FriendPlayerId).Returns(false);

        _friendUseCases.RemoveFriend(PlayerId, FriendUsername);

        _socialNotifier.DidNotReceive().NotifyFriendsChanged(Arg.Any<int>());
    }

    [Test]
    public void Test_RemoveFriend_NullUsername_ThrowsArgumentException()
    {
        _friendUsername = null;

        Assert.Throws<ArgumentException>(RemoveArrangedFriend);
    }

    [Test]
    public void Test_RemoveFriend_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _friendshipRepository.RemoveFriendship(PlayerId, FriendPlayerId)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(RemoveArrangedFriend);
    }

    [Test]
    public void Test_PublishStatus_PlayerHasFriend_NotifiesFriendPlayerIds()
    {
        _friendUseCases.PublishStatus(PlayerId);

        Assert.That(FindPublishedNotice().RecipientPlayerIds, Is.EqualTo(new List<int> { FriendPlayerId }));
    }

    [Test]
    public void Test_PublishStatus_PlayerHasFriend_NotifiesPlayerStatus()
    {
        _friendUseCases.PublishStatus(PlayerId);

        Assert.That(FindPublishedNotice().Status, Is.EqualTo(PlayerStatus.Online));
    }

    [Test]
    public void Test_PublishStatus_DatabaseUnavailable_DoesNotThrow()
    {
        _friendshipRepository.GetFriends(PlayerId).Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.DoesNotThrow(PublishPlayerStatus);
    }

    [Test]
    public void Test_PublishStatus_DatabaseUnavailable_DoesNotNotify()
    {
        _friendshipRepository.GetFriends(PlayerId).Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        _friendUseCases.PublishStatus(PlayerId);

        _socialNotifier.DidNotReceive().NotifyFriendStatusChanged(Arg.Any<FriendStatusNotice>());
    }

    private static string CreateText(int length)
    {
        string text = new string(FillerCharacter, length);
        return text;
    }

    private void CreateFriendUseCases()
    {
        _friendUseCases = new FriendUseCases(_friendshipRepository, _presenceUseCases, _socialNotifier);
    }

    private void GetPlayerFriends()
    {
        _friendUseCases.GetFriends(PlayerId);
    }

    private void GetPlayerPendingRequests()
    {
        _friendUseCases.GetPendingRequests(PlayerId);
    }

    private void SearchPlayersWithArrangedFragment()
    {
        _friendUseCases.SearchPlayers(PlayerId, _usernameFragment);
    }

    private void SendRequestToArrangedFriend()
    {
        _friendUseCases.SendFriendRequest(PlayerId, _friendUsername);
    }

    private void AcceptArrangedRequest()
    {
        _friendUseCases.AcceptFriendRequest(FriendPlayerId, RequestId);
    }

    private void RejectArrangedRequest()
    {
        _friendUseCases.RejectFriendRequest(FriendPlayerId, RequestId);
    }

    private void RemoveArrangedFriend()
    {
        _friendUseCases.RemoveFriend(PlayerId, _friendUsername);
    }

    private void PublishPlayerStatus()
    {
        _friendUseCases.PublishStatus(PlayerId);
    }

    private FriendStatusNotice FindPublishedNotice()
    {
        ICall notifyCall = _socialNotifier.ReceivedCalls().Single();
        FriendStatusNotice notice = (FriendStatusNotice)notifyCall.GetArguments()[NoticeArgumentIndex];
        return notice;
    }
}
