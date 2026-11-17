using System;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.UseCases;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace DominoTrainGame.Server.Application.Tests.Unit.UseCases;

[TestFixture]
public sealed class FriendUseCasesRequestTests
{
    private const int RequesterPlayerId = 7;
    private const int AddresseePlayerId = 12;
    private const int RequestId = 30;
    private const int UnknownRequestId = 99;
    private const int ExpectedCallCount = 1;
    private const int MaximumUsernameLength = 45;
    private const int TooLongUsernameLength = MaximumUsernameLength + 1;
    private const char FillerCharacter = 'a';
    private const string RequesterUsername = "samuel";
    private const string AddresseeUsername = "denisse";
    private const string PaddedAddresseeUsername = "  denisse  ";
    private const string UnknownUsername = "nobody";
    private const string DatabaseErrorMessage = "The database is unavailable.";

    private IFriendshipRepository _friendshipRepository;
    private ISocialNotifier _socialNotifier;
    private FriendUseCases _friendUseCases;
    private string _addresseeUsername;

    [SetUp]
    public void SetUp()
    {
        _friendshipRepository = Substitute.For<IFriendshipRepository>();
        _socialNotifier = Substitute.For<ISocialNotifier>();
        IPresenceUseCases presenceUseCases = Substitute.For<IPresenceUseCases>();

        _friendshipRepository.FindRegisteredPlayer(Arg.Any<string>()).Returns(PlayerIdentity.Empty);
        _friendshipRepository.FindRegisteredPlayer(RequesterUsername)
            .Returns(new PlayerIdentity(RequesterPlayerId, RequesterUsername));
        _friendshipRepository.FindRegisteredPlayer(AddresseeUsername)
            .Returns(new PlayerIdentity(AddresseePlayerId, AddresseeUsername));
        _friendshipRepository.GetFriendshipState(RequesterPlayerId, AddresseePlayerId).Returns(FriendshipState.None);
        _friendshipRepository.CreateRequest(RequesterPlayerId, AddresseePlayerId).Returns(true);
        _friendshipRepository.AcceptRequest(Arg.Any<int>(), Arg.Any<int>()).Returns(Array.Empty<int>());
        _friendshipRepository.AcceptRequest(RequestId, AddresseePlayerId).Returns(new[] { RequesterPlayerId });
        _friendshipRepository.RejectRequest(RequestId, AddresseePlayerId).Returns(true);

        _addresseeUsername = AddresseeUsername;
        _friendUseCases = new FriendUseCases(_friendshipRepository, presenceUseCases, _socialNotifier);
    }

    [Test]
    public void Test_SendFriendRequest_NewAddressee_ReturnsSuccess()
    {
        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(RequesterPlayerId, AddresseeUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.Success));
    }

    [Test]
    public void Test_SendFriendRequest_NewAddressee_CreatesRequest()
    {
        _friendUseCases.SendFriendRequest(RequesterPlayerId, AddresseeUsername);

        _friendshipRepository.Received(ExpectedCallCount).CreateRequest(RequesterPlayerId, AddresseePlayerId);
    }

    [Test]
    public void Test_SendFriendRequest_NewAddressee_NotifiesAddressee()
    {
        _friendUseCases.SendFriendRequest(RequesterPlayerId, AddresseeUsername);

        _socialNotifier.Received(ExpectedCallCount).NotifyFriendsChanged(AddresseePlayerId);
    }

    [Test]
    public void Test_SendFriendRequest_PaddedUsername_SearchesTrimmedUsername()
    {
        _friendUseCases.SendFriendRequest(RequesterPlayerId, PaddedAddresseeUsername);

        _friendshipRepository.Received(ExpectedCallCount).FindRegisteredPlayer(AddresseeUsername);
    }

    [Test]
    public void Test_SendFriendRequest_UnknownPlayer_ReturnsPlayerNotFound()
    {
        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(RequesterPlayerId, UnknownUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.PlayerNotFound));
    }

    [Test]
    public void Test_SendFriendRequest_UnknownPlayer_DoesNotCreateRequest()
    {
        _friendUseCases.SendFriendRequest(RequesterPlayerId, UnknownUsername);

        _friendshipRepository.DidNotReceive().CreateRequest(Arg.Any<int>(), Arg.Any<int>());
    }

    [Test]
    public void Test_SendFriendRequest_OwnUsername_ReturnsCannotTargetSelf()
    {
        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(RequesterPlayerId, RequesterUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.CannotTargetSelf));
    }

    [Test]
    public void Test_SendFriendRequest_AlreadyFriends_ReturnsAlreadyFriends()
    {
        _friendshipRepository.GetFriendshipState(RequesterPlayerId, AddresseePlayerId)
            .Returns(FriendshipState.Accepted);

        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(RequesterPlayerId, AddresseeUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.AlreadyFriends));
    }

    [Test]
    public void Test_SendFriendRequest_AlreadyFriends_DoesNotNotify()
    {
        _friendshipRepository.GetFriendshipState(RequesterPlayerId, AddresseePlayerId)
            .Returns(FriendshipState.Accepted);

        _friendUseCases.SendFriendRequest(RequesterPlayerId, AddresseeUsername);

        _socialNotifier.DidNotReceive().NotifyFriendsChanged(Arg.Any<int>());
    }

    [Test]
    public void Test_SendFriendRequest_PendingRequest_ReturnsRequestAlreadyExists()
    {
        _friendshipRepository.GetFriendshipState(RequesterPlayerId, AddresseePlayerId)
            .Returns(FriendshipState.Pending);

        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(RequesterPlayerId, AddresseeUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.RequestAlreadyExists));
    }

    [Test]
    public void Test_SendFriendRequest_AddresseeBlocked_ReturnsPlayerBlocked()
    {
        _friendshipRepository.GetFriendshipState(RequesterPlayerId, AddresseePlayerId)
            .Returns(FriendshipState.Blocked);

        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(RequesterPlayerId, AddresseeUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.PlayerBlocked));
    }

    [Test]
    public void Test_SendFriendRequest_PreviouslyRejected_ReturnsSuccess()
    {
        _friendshipRepository.GetFriendshipState(RequesterPlayerId, AddresseePlayerId)
            .Returns(FriendshipState.Rejected);

        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(RequesterPlayerId, AddresseeUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.Success));
    }

    [Test]
    public void Test_SendFriendRequest_RequestNotCreated_ReturnsRequestAlreadyExists()
    {
        _friendshipRepository.CreateRequest(RequesterPlayerId, AddresseePlayerId).Returns(false);

        SendFriendRequestStatus status = _friendUseCases.SendFriendRequest(RequesterPlayerId, AddresseeUsername);

        Assert.That(status, Is.EqualTo(SendFriendRequestStatus.RequestAlreadyExists));
    }

    [Test]
    public void Test_SendFriendRequest_NullUsername_ThrowsArgumentException()
    {
        _addresseeUsername = null;

        Assert.Throws<ArgumentException>(SendRequestToArrangedAddressee);
    }

    [Test]
    public void Test_SendFriendRequest_TooLongUsername_ThrowsArgumentException()
    {
        _addresseeUsername = new string(FillerCharacter, TooLongUsernameLength);

        Assert.Throws<ArgumentException>(SendRequestToArrangedAddressee);
    }

    [Test]
    public void Test_SendFriendRequest_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _friendshipRepository.FindRegisteredPlayer(AddresseeUsername)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(SendRequestToArrangedAddressee);
    }

    [Test]
    public void Test_AcceptFriendRequest_ExistingRequest_ReturnsTrue()
    {
        bool isAccepted = _friendUseCases.AcceptFriendRequest(AddresseePlayerId, RequestId);

        Assert.That(isAccepted, Is.True);
    }

    [Test]
    public void Test_AcceptFriendRequest_ExistingRequest_NotifiesRequester()
    {
        _friendUseCases.AcceptFriendRequest(AddresseePlayerId, RequestId);

        _socialNotifier.Received(ExpectedCallCount).NotifyFriendsChanged(RequesterPlayerId);
    }

    [Test]
    public void Test_AcceptFriendRequest_RequestNotFound_ReturnsFalse()
    {
        bool isAccepted = _friendUseCases.AcceptFriendRequest(AddresseePlayerId, UnknownRequestId);

        Assert.That(isAccepted, Is.False);
    }

    [Test]
    public void Test_AcceptFriendRequest_RequestNotFound_DoesNotNotify()
    {
        _friendUseCases.AcceptFriendRequest(AddresseePlayerId, UnknownRequestId);

        _socialNotifier.DidNotReceive().NotifyFriendsChanged(Arg.Any<int>());
    }

    [Test]
    public void Test_AcceptFriendRequest_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _friendshipRepository.AcceptRequest(RequestId, AddresseePlayerId)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(AcceptArrangedRequest);
    }

    [Test]
    public void Test_RejectFriendRequest_ExistingRequest_ReturnsTrue()
    {
        bool isRejected = _friendUseCases.RejectFriendRequest(AddresseePlayerId, RequestId);

        Assert.That(isRejected, Is.True);
    }

    [Test]
    public void Test_RejectFriendRequest_RequestNotFound_ReturnsFalse()
    {
        bool isRejected = _friendUseCases.RejectFriendRequest(AddresseePlayerId, UnknownRequestId);

        Assert.That(isRejected, Is.False);
    }

    [Test]
    public void Test_RejectFriendRequest_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _friendshipRepository.RejectRequest(RequestId, AddresseePlayerId)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(RejectArrangedRequest);
    }

    private void SendRequestToArrangedAddressee()
    {
        _friendUseCases.SendFriendRequest(RequesterPlayerId, _addresseeUsername);
    }

    private void AcceptArrangedRequest()
    {
        _friendUseCases.AcceptFriendRequest(AddresseePlayerId, RequestId);
    }

    private void RejectArrangedRequest()
    {
        _friendUseCases.RejectFriendRequest(AddresseePlayerId, RequestId);
    }
}
