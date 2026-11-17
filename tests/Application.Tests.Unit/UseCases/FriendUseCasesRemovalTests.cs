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
public sealed class FriendUseCasesRemovalTests
{
    private const int PlayerId = 7;
    private const int FriendPlayerId = 12;
    private const int ExpectedCallCount = 1;
    private const string FriendUsername = "denisse";
    private const string PaddedFriendUsername = "  denisse  ";
    private const string UnknownUsername = "nobody";
    private const string DatabaseErrorMessage = "The database is unavailable.";

    private IFriendshipRepository _friendshipRepository;
    private ISocialNotifier _socialNotifier;
    private FriendUseCases _friendUseCases;
    private string _friendUsername;

    [SetUp]
    public void SetUp()
    {
        _friendshipRepository = Substitute.For<IFriendshipRepository>();
        _socialNotifier = Substitute.For<ISocialNotifier>();
        IPresenceUseCases presenceUseCases = Substitute.For<IPresenceUseCases>();

        _friendshipRepository.FindRegisteredPlayer(Arg.Any<string>()).Returns(PlayerIdentity.Empty);
        _friendshipRepository.FindRegisteredPlayer(FriendUsername)
            .Returns(new PlayerIdentity(FriendPlayerId, FriendUsername));
        _friendshipRepository.RemoveFriendship(PlayerId, FriendPlayerId).Returns(true);

        _friendUsername = FriendUsername;
        _friendUseCases = new FriendUseCases(_friendshipRepository, presenceUseCases, _socialNotifier);
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

    private void RemoveArrangedFriend()
    {
        _friendUseCases.RemoveFriend(PlayerId, _friendUsername);
    }
}
