using System;
using System.ServiceModel;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.Faults;
using DominoTrainGame.Server.Service.Sessions;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DominoTrainGame.Server.Service.Tests.Unit.Sessions;

[TestFixture]
public sealed class PlayerSessionManagerTests
{
    private const int PlayerId = 7;
    private const int GuestPlayerId = 21;
    private const int TokenLifetimeMinutes = 30;
    private const int IssueYear = 2026;
    private const int IssueMonth = 11;
    private const int IssueDay = 24;
    private const string PlayerUsername = "samuel";
    private const string GuestUsername = "Guest123456";
    private const string ChannelSessionId = "urn:uuid:channel-session";
    private const string UnknownToken = "bm90LWFuLWlzc3VlZC10b2tlbg==";
    private const string EmptyText = "";
    private const string FaultDetailPropertyName = nameof(FaultException<GameServiceFault>.Detail);
    private const string FaultCodePropertyName = nameof(GameServiceFault.Code);

    private static readonly DateTime _issuedAt = new DateTime(IssueYear, IssueMonth, IssueDay);

    private IClock _clock;
    private PlayerSessionRegistry<ISocialClientCallback> _sessions;
    private SessionTokenRegistry _sessionTokens;
    private PlayerSessionManager<ISocialClientCallback> _sessionManager;
    private PlayerIdentity _player;
    private string _sessionToken;

    [SetUp]
    public void SetUp()
    {
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(_issuedAt);
        _sessions = new PlayerSessionRegistry<ISocialClientCallback>();
        _sessionTokens = new SessionTokenRegistry(_clock);
        _player = new PlayerIdentity(PlayerId, PlayerUsername);

        CreateSessionManager();
        _sessionToken = _sessionTokens.Issue(_player);
    }

    [Test]
    public void Test_Constructor_NullSessions_ThrowsArgumentNullException()
    {
        _sessions = null;

        Assert.Throws<ArgumentNullException>(CreateSessionManager);
    }

    [Test]
    public void Test_Constructor_NullSessionTokens_ThrowsArgumentNullException()
    {
        _sessionTokens = null;

        Assert.Throws<ArgumentNullException>(CreateSessionManager);
    }

    [Test]
    public void Test_IssueToken_OfflinePlayer_ReturnsTokenOfThatPlayer()
    {
        string sessionToken = _sessionManager.IssueToken(_player);

        PlayerIdentity tokenOwner = _sessionTokens.Resolve(sessionToken);
        Assert.That(tokenOwner.PlayerId, Is.EqualTo(PlayerId));
    }

    [Test]
    public void Test_IssueToken_NullPlayer_ThrowsArgumentNullException()
    {
        _player = null;

        Assert.Throws<ArgumentNullException>(IssueArrangedToken);
    }

    [Test]
    public void Test_IssueToken_PlayerAlreadyConnected_ThrowsAlreadyConnectedFault()
    {
        _sessions.TryAdd(new PlayerSession<ISocialClientCallback>(
            ChannelSessionId,
            _player,
            Substitute.For<ISocialClientCallback>()));

        Assert.That(IssueArrangedToken, ThrowsFault(GameServiceFaultCode.AlreadyConnected));
    }

    [Test]
    public void Test_Connect_UnknownToken_ThrowsInvalidSessionTokenFault()
    {
        _sessionToken = UnknownToken;

        Assert.That(ConnectWithArrangedToken, ThrowsFault(GameServiceFaultCode.InvalidSessionToken));
    }

    [TestCase(EmptyText)]
    [TestCase(null)]
    public void Test_Connect_BlankToken_ThrowsInvalidSessionTokenFault(string blankToken)
    {
        _sessionToken = blankToken;

        Assert.That(ConnectWithArrangedToken, ThrowsFault(GameServiceFaultCode.InvalidSessionToken));
    }

    [Test]
    public void Test_Connect_ExpiredToken_ThrowsInvalidSessionTokenFault()
    {
        _clock.UtcNow.Returns(_issuedAt.AddMinutes(TokenLifetimeMinutes));

        Assert.That(ConnectWithArrangedToken, ThrowsFault(GameServiceFaultCode.InvalidSessionToken));
    }

    [Test]
    public void Test_Connect_RevokedToken_ThrowsInvalidSessionTokenFault()
    {
        _sessionTokens.RevokePlayer(PlayerId);

        Assert.That(ConnectWithArrangedToken, ThrowsFault(GameServiceFaultCode.InvalidSessionToken));
    }

    [Test]
    public void Test_ConnectRegisteredPlayer_GuestToken_ThrowsGuestNotAllowedFault()
    {
        _sessionToken = _sessionTokens.Issue(PlayerIdentity.CreateGuest(GuestPlayerId, GuestUsername));

        Assert.That(ConnectRegisteredPlayerWithArrangedToken, ThrowsFault(GameServiceFaultCode.GuestNotAllowed));
    }

    [Test]
    public void Test_ConnectRegisteredPlayer_UnknownToken_ThrowsInvalidSessionTokenFault()
    {
        _sessionToken = UnknownToken;

        Assert.That(
            ConnectRegisteredPlayerWithArrangedToken,
            ThrowsFault(GameServiceFaultCode.InvalidSessionToken));
    }

    private static Constraint ThrowsFault(GameServiceFaultCode faultCode)
    {
        Constraint faultConstraint = Throws.TypeOf<FaultException<GameServiceFault>>()
            .With.Property(FaultDetailPropertyName)
            .With.Property(FaultCodePropertyName)
            .EqualTo(faultCode);
        return faultConstraint;
    }

    private void CreateSessionManager()
    {
        _sessionManager = new PlayerSessionManager<ISocialClientCallback>(_sessions, _sessionTokens);
    }

    private void IssueArrangedToken()
    {
        _sessionManager.IssueToken(_player);
    }

    private void ConnectWithArrangedToken()
    {
        _sessionManager.Connect(_sessionToken);
    }

    private void ConnectRegisteredPlayerWithArrangedToken()
    {
        _sessionManager.ConnectRegisteredPlayer(_sessionToken);
    }
}
