using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
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
    private const int ExpectedCallCount = 1;
    private const int NoticeArgumentIndex = 0;
    private const int MaximumUsernameLength = 45;
    private const int TooLongUsernameLength = MaximumUsernameLength + 1;
    private const char FillerCharacter = 'a';
    private const string FriendUsername = "denisse";
    private const string RequesterUsername = "carlos";
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
        _presenceUseCases.GetStatus(FriendPlayerId).Returns(PlayerStatus.InMatch);
        _presenceUseCases.GetStatus(PlayerId).Returns(PlayerStatus.Online);

        _usernameFragment = UsernameFragment;
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
