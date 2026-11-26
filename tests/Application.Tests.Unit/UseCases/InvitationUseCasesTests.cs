using System;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.State;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Shared.Rules;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace DominoTrainGame.Server.Application.Tests.Unit.UseCases;

[TestFixture]
public sealed class InvitationUseCasesTests
{
    private const int InviterPlayerId = 21;
    private const int FriendPlayerId = 22;
    private const int PlayerWithoutLobbyId = 23;
    private const int FirstMemberPlayerId = 30;
    private const int ExpectedCallCount = 1;
    private const int TooLongUsernameLength = GameRules.MaximumUsernameLength + 1;
    private const char FillerCharacter = 'a';
    private const string LobbyCode = "ABC123";
    private const string InviterUsername = "samuel";
    private const string GuestUsername = "Guest123456";
    private const string MemberUsername = "member";
    private const string FriendUsername = "denisse";
    private const string PaddedFriendUsername = "  denisse  ";
    private const string UnknownUsername = "nobody";
    private const string EmptyText = "";
    private const string DatabaseErrorMessage = "The database is unavailable.";

    private LobbyRegistry _lobbyRegistry;
    private IFriendshipRepository _friendshipRepository;
    private IGameNotifier _gameNotifier;
    private InvitationUseCases _invitationUseCases;
    private PlayerIdentity _inviter;
    private string _friendUsername;

    [SetUp]
    public void SetUp()
    {
        ILobbyCodeGenerator codeGenerator = Substitute.For<ILobbyCodeGenerator>();
        codeGenerator.Generate().Returns(LobbyCode);
        codeGenerator.IsValid(Arg.Any<string>()).Returns(true);

        _lobbyRegistry = new LobbyRegistry(codeGenerator);
        _friendshipRepository = Substitute.For<IFriendshipRepository>();
        _gameNotifier = Substitute.For<IGameNotifier>();

        _friendshipRepository.FindRegisteredPlayer(Arg.Any<string>()).Returns(PlayerIdentity.Empty);
        _friendshipRepository.FindRegisteredPlayer(FriendUsername)
            .Returns(new PlayerIdentity(FriendPlayerId, FriendUsername));
        _friendshipRepository.GetFriendshipState(InviterPlayerId, FriendPlayerId).Returns(FriendshipState.Accepted);
        _gameNotifier.TryNotifyLobbyInvitation(Arg.Any<LobbyInvitationNotice>()).Returns(true);

        _inviter = new PlayerIdentity(InviterPlayerId, InviterUsername);
        _lobbyRegistry.Create(_inviter);
        _friendUsername = FriendUsername;
        CreateInvitationUseCases();
    }

    [Test]
    public void Test_Constructor_NullLobbyRegistry_ThrowsArgumentNullException()
    {
        _lobbyRegistry = null;

        Assert.Throws<ArgumentNullException>(CreateInvitationUseCases);
    }

    [Test]
    public void Test_Constructor_NullFriendshipRepository_ThrowsArgumentNullException()
    {
        _friendshipRepository = null;

        Assert.Throws<ArgumentNullException>(CreateInvitationUseCases);
    }

    [Test]
    public void Test_Constructor_NullGameNotifier_ThrowsArgumentNullException()
    {
        _gameNotifier = null;

        Assert.Throws<ArgumentNullException>(CreateInvitationUseCases);
    }

    [Test]
    public void Test_InviteFriend_NullInviter_ThrowsArgumentNullException()
    {
        _inviter = null;

        Assert.Throws<ArgumentNullException>(InviteArrangedFriend);
    }

    [Test]
    public void Test_InviteFriend_GuestInviter_ReturnsGuestNotAllowed()
    {
        PlayerIdentity guest = PlayerIdentity.CreateGuest(InviterPlayerId, GuestUsername);

        InviteFriendStatus status = _invitationUseCases.InviteFriend(guest, FriendUsername);

        Assert.That(status, Is.EqualTo(InviteFriendStatus.GuestNotAllowed));
    }

    [Test]
    public void Test_InviteFriend_GuestInviter_DoesNotSearchPlayer()
    {
        PlayerIdentity guest = PlayerIdentity.CreateGuest(InviterPlayerId, GuestUsername);

        _invitationUseCases.InviteFriend(guest, FriendUsername);

        _friendshipRepository.DidNotReceive().FindRegisteredPlayer(Arg.Any<string>());
    }

    [Test]
    public void Test_InviteFriend_GuestInviter_DoesNotNotify()
    {
        PlayerIdentity guest = PlayerIdentity.CreateGuest(InviterPlayerId, GuestUsername);

        _invitationUseCases.InviteFriend(guest, FriendUsername);

        _gameNotifier.DidNotReceive().TryNotifyLobbyInvitation(Arg.Any<LobbyInvitationNotice>());
    }

    [Test]
    public void Test_InviteFriend_GuestInviterWithNullUsername_ReturnsGuestNotAllowed()
    {
        PlayerIdentity guest = PlayerIdentity.CreateGuest(InviterPlayerId, GuestUsername);

        InviteFriendStatus status = _invitationUseCases.InviteFriend(guest, null);

        Assert.That(status, Is.EqualTo(InviteFriendStatus.GuestNotAllowed));
    }

    [Test]
    public void Test_InviteFriend_InviterWithoutLobby_ReturnsLobbyNotFound()
    {
        PlayerIdentity playerWithoutLobby = new PlayerIdentity(PlayerWithoutLobbyId, InviterUsername);

        InviteFriendStatus status = _invitationUseCases.InviteFriend(playerWithoutLobby, FriendUsername);

        Assert.That(status, Is.EqualTo(InviteFriendStatus.LobbyNotFound));
    }

    [Test]
    public void Test_InviteFriend_InviterWithoutLobby_DoesNotSearchPlayer()
    {
        PlayerIdentity playerWithoutLobby = new PlayerIdentity(PlayerWithoutLobbyId, InviterUsername);

        _invitationUseCases.InviteFriend(playerWithoutLobby, FriendUsername);

        _friendshipRepository.DidNotReceive().FindRegisteredPlayer(Arg.Any<string>());
    }

    [Test]
    public void Test_InviteFriend_FullLobby_ReturnsLobbyFull()
    {
        FillInviterLobby();

        InviteFriendStatus status = _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        Assert.That(status, Is.EqualTo(InviteFriendStatus.LobbyFull));
    }

    [Test]
    public void Test_InviteFriend_FullLobby_DoesNotNotify()
    {
        FillInviterLobby();

        _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        _gameNotifier.DidNotReceive().TryNotifyLobbyInvitation(Arg.Any<LobbyInvitationNotice>());
    }

    [Test]
    public void Test_InviteFriend_UnknownPlayer_ReturnsPlayerNotFound()
    {
        InviteFriendStatus status = _invitationUseCases.InviteFriend(_inviter, UnknownUsername);

        Assert.That(status, Is.EqualTo(InviteFriendStatus.PlayerNotFound));
    }

    [Test]
    public void Test_InviteFriend_UnknownPlayer_DoesNotNotify()
    {
        _invitationUseCases.InviteFriend(_inviter, UnknownUsername);

        _gameNotifier.DidNotReceive().TryNotifyLobbyInvitation(Arg.Any<LobbyInvitationNotice>());
    }

    [Test]
    public void Test_InviteFriend_PaddedUsername_SearchesTrimmedUsername()
    {
        _invitationUseCases.InviteFriend(_inviter, PaddedFriendUsername);

        _friendshipRepository.Received(ExpectedCallCount).FindRegisteredPlayer(FriendUsername);
    }

    [Test]
    public void Test_InviteFriend_NullUsername_ThrowsArgumentException()
    {
        _friendUsername = null;

        Assert.Throws<ArgumentException>(InviteArrangedFriend);
    }

    [Test]
    public void Test_InviteFriend_EmptyUsername_ThrowsArgumentException()
    {
        _friendUsername = EmptyText;

        Assert.Throws<ArgumentException>(InviteArrangedFriend);
    }

    [Test]
    public void Test_InviteFriend_TooLongUsername_ThrowsArgumentException()
    {
        _friendUsername = new string(FillerCharacter, TooLongUsernameLength);

        Assert.Throws<ArgumentException>(InviteArrangedFriend);
    }

    [Test]
    public void Test_InviteFriend_DatabaseUnavailableOnSearch_ThrowsDatabaseUnavailableException()
    {
        _friendshipRepository.FindRegisteredPlayer(FriendUsername)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(InviteArrangedFriend);
    }

    [Test]
    public void Test_InviteFriend_ConnectedFriend_ReturnsSuccess()
    {
        InviteFriendStatus status = _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        Assert.That(status, Is.EqualTo(InviteFriendStatus.Success));
    }

    [Test]
    public void Test_InviteFriend_ConnectedFriend_NotifiesOnce()
    {
        _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        _gameNotifier.Received(ExpectedCallCount).TryNotifyLobbyInvitation(Arg.Any<LobbyInvitationNotice>());
    }

    [Test]
    public void Test_InviteFriend_ConnectedFriend_NotifiesFriend()
    {
        _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        Assert.That(GetSentNotice().RecipientPlayerId, Is.EqualTo(FriendPlayerId));
    }

    [Test]
    public void Test_InviteFriend_ConnectedFriend_SendsLobbyCode()
    {
        _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        Assert.That(GetSentNotice().Code, Is.EqualTo(LobbyCode));
    }

    [Test]
    public void Test_InviteFriend_ConnectedFriend_SendsInviterUsername()
    {
        _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        Assert.That(GetSentNotice().Inviter.Username, Is.EqualTo(InviterUsername));
    }

    [Test]
    public void Test_InviteFriend_DisconnectedFriend_ReturnsPlayerOffline()
    {
        _gameNotifier.TryNotifyLobbyInvitation(Arg.Any<LobbyInvitationNotice>()).Returns(false);

        InviteFriendStatus status = _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        Assert.That(status, Is.EqualTo(InviteFriendStatus.PlayerOffline));
    }

    [Test]
    public void Test_InviteFriend_NoFriendship_ReturnsNotFriends()
    {
        _friendshipRepository.GetFriendshipState(InviterPlayerId, FriendPlayerId).Returns(FriendshipState.None);

        InviteFriendStatus status = _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        Assert.That(status, Is.EqualTo(InviteFriendStatus.NotFriends));
    }

    [Test]
    public void Test_InviteFriend_PendingFriendship_ReturnsNotFriends()
    {
        _friendshipRepository.GetFriendshipState(InviterPlayerId, FriendPlayerId).Returns(FriendshipState.Pending);

        InviteFriendStatus status = _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        Assert.That(status, Is.EqualTo(InviteFriendStatus.NotFriends));
    }

    [Test]
    public void Test_InviteFriend_RejectedFriendship_ReturnsNotFriends()
    {
        _friendshipRepository.GetFriendshipState(InviterPlayerId, FriendPlayerId).Returns(FriendshipState.Rejected);

        InviteFriendStatus status = _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        Assert.That(status, Is.EqualTo(InviteFriendStatus.NotFriends));
    }

    [Test]
    public void Test_InviteFriend_BlockedFriendship_ReturnsNotFriends()
    {
        _friendshipRepository.GetFriendshipState(InviterPlayerId, FriendPlayerId).Returns(FriendshipState.Blocked);

        InviteFriendStatus status = _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        Assert.That(status, Is.EqualTo(InviteFriendStatus.NotFriends));
    }

    [Test]
    public void Test_InviteFriend_PendingFriendship_DoesNotNotify()
    {
        _friendshipRepository.GetFriendshipState(InviterPlayerId, FriendPlayerId).Returns(FriendshipState.Pending);

        _invitationUseCases.InviteFriend(_inviter, FriendUsername);

        _gameNotifier.DidNotReceive().TryNotifyLobbyInvitation(Arg.Any<LobbyInvitationNotice>());
    }

    [Test]
    public void Test_InviteFriend_DatabaseUnavailableOnFriendship_ThrowsDatabaseUnavailableException()
    {
        _friendshipRepository.GetFriendshipState(InviterPlayerId, FriendPlayerId)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(InviteArrangedFriend);
    }

    private void CreateInvitationUseCases()
    {
        _invitationUseCases = new InvitationUseCases(_lobbyRegistry, _friendshipRepository, _gameNotifier);
    }

    private void InviteArrangedFriend()
    {
        _invitationUseCases.InviteFriend(_inviter, _friendUsername);
    }

    private void FillInviterLobby()
    {
        LobbyState lobby = _lobbyRegistry.FindLobbyOfPlayer(InviterPlayerId);
        int lastMemberPlayerId = FirstMemberPlayerId + lobby.MaximumPlayers - lobby.Roster.Count;

        for (int memberPlayerId = FirstMemberPlayerId; memberPlayerId < lastMemberPlayerId; memberPlayerId++)
        {
            _lobbyRegistry.Join(new PlayerIdentity(memberPlayerId, MemberUsername), LobbyCode);
        }
    }

    private LobbyInvitationNotice GetSentNotice()
    {
        ICall notifyCall = _gameNotifier.ReceivedCalls().Single();
        LobbyInvitationNotice notice = (LobbyInvitationNotice)notifyCall.GetArguments().Single();
        return notice;
    }
}
