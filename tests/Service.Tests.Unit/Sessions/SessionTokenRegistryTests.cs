using System;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Service.Sessions;
using NSubstitute;
using NUnit.Framework;

namespace DominoTrainGame.Server.Service.Tests.Unit.Sessions;

[TestFixture]
public sealed class SessionTokenRegistryTests
{
    private const int PlayerId = 7;
    private const int OtherPlayerId = 12;
    private const int GuestPlayerId = 21;
    private const int EncodedTokenLength = 44;
    private const int MinutesBeforeExpiration = 29;
    private const int TokenLifetimeMinutes = 30;
    private const int RenewalMinute = 20;
    private const int MinuteAfterFirstExpiration = 40;
    private const int IssueYear = 2026;
    private const int IssueMonth = 11;
    private const int IssueDay = 24;
    private const string PlayerUsername = "samuel";
    private const string OtherPlayerUsername = "denisse";
    private const string RenamedPlayerUsername = "samuel_uv";
    private const string GuestUsername = "Guest123456";
    private const string UnknownToken = "bm90LWFuLWlzc3VlZC10b2tlbg==";
    private const string EmptyText = "";

    private static readonly DateTime _issuedAt = new DateTime(IssueYear, IssueMonth, IssueDay);

    private IClock _clock;
    private SessionTokenRegistry _sessionTokens;
    private PlayerIdentity _player;
    private string _sessionToken;

    [SetUp]
    public void SetUp()
    {
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(_issuedAt);
        _player = new PlayerIdentity(PlayerId, PlayerUsername);

        CreateSessionTokenRegistry();
        _sessionToken = _sessionTokens.Issue(_player);
    }

    [Test]
    public void Test_Constructor_NullClock_ThrowsArgumentNullException()
    {
        _clock = null;

        Assert.Throws<ArgumentNullException>(CreateSessionTokenRegistry);
    }

    [Test]
    public void Test_Issue_NullPlayer_ThrowsArgumentNullException()
    {
        _player = null;

        Assert.Throws<ArgumentNullException>(IssueArrangedToken);
    }

    [Test]
    public void Test_Issue_RegisteredPlayer_ReturnsEncodedRandomToken()
    {
        string sessionToken = _sessionTokens.Issue(_player);

        Assert.That(sessionToken, Has.Length.EqualTo(EncodedTokenLength));
    }

    [Test]
    public void Test_Issue_OtherPlayer_ReturnsDifferentToken()
    {
        string otherSessionToken = _sessionTokens.Issue(new PlayerIdentity(OtherPlayerId, OtherPlayerUsername));

        Assert.That(otherSessionToken, Is.Not.EqualTo(_sessionToken));
    }

    [Test]
    public void Test_Issue_SamePlayerAgain_ReturnsDifferentToken()
    {
        string newSessionToken = _sessionTokens.Issue(_player);

        Assert.That(newSessionToken, Is.Not.EqualTo(_sessionToken));
    }

    [Test]
    public void Test_Resolve_IssuedToken_ReturnsPlayerId()
    {
        PlayerIdentity player = _sessionTokens.Resolve(_sessionToken);

        Assert.That(player.PlayerId, Is.EqualTo(PlayerId));
    }

    [Test]
    public void Test_Resolve_GuestToken_ReturnsGuestPlayer()
    {
        string guestToken = _sessionTokens.Issue(PlayerIdentity.CreateGuest(GuestPlayerId, GuestUsername));

        PlayerIdentity player = _sessionTokens.Resolve(guestToken);

        Assert.That(player.IsGuest, Is.True);
    }

    [Test]
    public void Test_Resolve_UnknownToken_ReturnsEmptyIdentity()
    {
        PlayerIdentity player = _sessionTokens.Resolve(UnknownToken);

        Assert.That(player.IsEmpty, Is.True);
    }

    [TestCase(EmptyText)]
    [TestCase(null)]
    public void Test_Resolve_BlankToken_ReturnsEmptyIdentity(string blankToken)
    {
        PlayerIdentity player = _sessionTokens.Resolve(blankToken);

        Assert.That(player.IsEmpty, Is.True);
    }

    [Test]
    public void Test_Resolve_BeforeExpiration_ReturnsPlayerId()
    {
        MoveClockTo(MinutesBeforeExpiration);

        PlayerIdentity player = _sessionTokens.Resolve(_sessionToken);

        Assert.That(player.PlayerId, Is.EqualTo(PlayerId));
    }

    [Test]
    public void Test_Resolve_AtExpiration_ReturnsEmptyIdentity()
    {
        MoveClockTo(TokenLifetimeMinutes);

        PlayerIdentity player = _sessionTokens.Resolve(_sessionToken);

        Assert.That(player.IsEmpty, Is.True);
    }

    [Test]
    public void Test_Resolve_TokenReplacedByNewLogin_ReturnsEmptyIdentity()
    {
        _sessionTokens.Issue(_player);

        PlayerIdentity player = _sessionTokens.Resolve(_sessionToken);

        Assert.That(player.IsEmpty, Is.True);
    }

    [Test]
    public void Test_Resolve_TokenOfNewLogin_ReturnsPlayerId()
    {
        string newSessionToken = _sessionTokens.Issue(_player);

        PlayerIdentity player = _sessionTokens.Resolve(newSessionToken);

        Assert.That(player.PlayerId, Is.EqualTo(PlayerId));
    }

    [Test]
    public void Test_Resolve_RevokedToken_ReturnsEmptyIdentity()
    {
        _sessionTokens.RevokePlayer(PlayerId);

        PlayerIdentity player = _sessionTokens.Resolve(_sessionToken);

        Assert.That(player.IsEmpty, Is.True);
    }

    [Test]
    public void Test_Resolve_OtherPlayerRevoked_ReturnsPlayerId()
    {
        _sessionTokens.RevokePlayer(OtherPlayerId);

        PlayerIdentity player = _sessionTokens.Resolve(_sessionToken);

        Assert.That(player.PlayerId, Is.EqualTo(PlayerId));
    }

    [Test]
    public void Test_Resolve_RenewedToken_ExtendsExpiration()
    {
        MoveClockTo(RenewalMinute);
        _sessionTokens.Renew(_sessionToken);
        MoveClockTo(MinuteAfterFirstExpiration);

        PlayerIdentity player = _sessionTokens.Resolve(_sessionToken);

        Assert.That(player.PlayerId, Is.EqualTo(PlayerId));
    }

    [Test]
    public void Test_Renew_UnknownToken_DoesNotIssueToken()
    {
        _sessionTokens.Renew(UnknownToken);

        PlayerIdentity player = _sessionTokens.Resolve(UnknownToken);

        Assert.That(player.IsEmpty, Is.True);
    }

    [Test]
    public void Test_Renew_RevokedToken_KeepsTokenRevoked()
    {
        _sessionTokens.RevokePlayer(PlayerId);
        _sessionTokens.Renew(_sessionToken);

        PlayerIdentity player = _sessionTokens.Resolve(_sessionToken);

        Assert.That(player.IsEmpty, Is.True);
    }

    [Test]
    public void Test_Renew_NullToken_DoesNotThrow()
    {
        _sessionToken = null;

        Assert.DoesNotThrow(RenewArrangedToken);
    }

    [Test]
    public void Test_RevokePlayer_PlayerWithoutToken_DoesNotThrow()
    {
        _player = new PlayerIdentity(OtherPlayerId, OtherPlayerUsername);

        Assert.DoesNotThrow(RevokeArrangedPlayer);
    }

    [Test]
    public void Test_UpdateUsername_IssuedToken_ResolvesNewUsername()
    {
        _sessionTokens.UpdateUsername(PlayerId, RenamedPlayerUsername);

        PlayerIdentity player = _sessionTokens.Resolve(_sessionToken);

        Assert.That(player.Username, Is.EqualTo(RenamedPlayerUsername));
    }

    [Test]
    public void Test_UpdateUsername_IssuedToken_RenewsExpiration()
    {
        MoveClockTo(RenewalMinute);
        _sessionTokens.UpdateUsername(PlayerId, RenamedPlayerUsername);
        MoveClockTo(MinuteAfterFirstExpiration);

        PlayerIdentity player = _sessionTokens.Resolve(_sessionToken);

        Assert.That(player.PlayerId, Is.EqualTo(PlayerId));
    }

    [Test]
    public void Test_UpdateUsername_OtherPlayer_KeepsUsername()
    {
        _sessionTokens.UpdateUsername(OtherPlayerId, RenamedPlayerUsername);

        PlayerIdentity player = _sessionTokens.Resolve(_sessionToken);

        Assert.That(player.Username, Is.EqualTo(PlayerUsername));
    }

    private void CreateSessionTokenRegistry()
    {
        _sessionTokens = new SessionTokenRegistry(_clock);
    }

    private void IssueArrangedToken()
    {
        _sessionTokens.Issue(_player);
    }

    private void RenewArrangedToken()
    {
        _sessionTokens.Renew(_sessionToken);
    }

    private void RevokeArrangedPlayer()
    {
        _sessionTokens.RevokePlayer(_player.PlayerId);
    }

    private void MoveClockTo(int minutesAfterIssue)
    {
        _clock.UtcNow.Returns(_issuedAt.AddMinutes(minutesAfterIssue));
    }
}
