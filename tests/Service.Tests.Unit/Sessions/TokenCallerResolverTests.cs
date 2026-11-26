using System;
using System.ServiceModel;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Service.Faults;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Service.Tests.Unit.Sessions;

[TestFixture]
public sealed class TokenCallerResolverTests
{
    private const int PlayerId = 7;
    private const int GuestPlayerId = 21;
    private const int RenewalMinute = 20;
    private const int MinuteAfterFirstExpiration = 40;
    private const int IssueYear = 2026;
    private const int IssueMonth = 11;
    private const int IssueDay = 26;
    private const string PlayerUsername = "denisse";
    private const string GuestUsername = "Guest654321";
    private const string UnknownToken = "bm90LWFuLWlzc3VlZC10b2tlbg==";
    private const string FaultDetailPropertyName = nameof(FaultException<GameServiceFault>.Detail);
    private const string FaultCodePropertyName = nameof(GameServiceFault.Code);

    private static readonly DateTime _issuedAt;

    private IClock _clock;
    private SessionTokenRegistry _sessionTokens;
    private string _sessionToken;

    static TokenCallerResolverTests()
    {
        _issuedAt = new DateTime(IssueYear, IssueMonth, IssueDay);
    }

    [SetUp]
    public void SetUp()
    {
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(_issuedAt);
        _sessionTokens = new SessionTokenRegistry(_clock);
        _sessionToken = _sessionTokens.Issue(new PlayerIdentity(PlayerId, PlayerUsername));
    }

    [Test]
    public void Test_ResolveSignedInCaller_NullRegistry_ThrowsArgumentNullException()
    {
        _sessionTokens = null;

        Assert.Throws<ArgumentNullException>(ResolveArrangedSignedInCaller);
    }

    [Test]
    public void Test_ResolveSignedInCaller_IssuedToken_ReturnsCaller()
    {
        PlayerIdentity caller = TokenCallerResolver.ResolveSignedInCaller(_sessionTokens, _sessionToken);

        Assert.That(caller.PlayerId, Is.EqualTo(PlayerId));
    }

    [Test]
    public void Test_ResolveSignedInCaller_UnknownToken_ThrowsSessionNotFoundFault()
    {
        _sessionToken = UnknownToken;

        Assert.That(
            ResolveArrangedSignedInCaller,
            ThrowsFaultWithCode(GameServiceFaultCode.SessionNotFound));
    }

    [Test]
    public void Test_ResolveSignedInCaller_IssuedToken_RenewsSession()
    {
        MoveClockTo(RenewalMinute);
        TokenCallerResolver.ResolveSignedInCaller(_sessionTokens, _sessionToken);
        MoveClockTo(MinuteAfterFirstExpiration);

        PlayerIdentity caller = _sessionTokens.Resolve(_sessionToken);

        Assert.That(caller.PlayerId, Is.EqualTo(PlayerId));
    }

    [Test]
    public void Test_ResolveRegisteredCaller_RegisteredToken_ReturnsCaller()
    {
        PlayerIdentity caller = TokenCallerResolver.ResolveRegisteredCaller(_sessionTokens, _sessionToken);

        Assert.That(caller.PlayerId, Is.EqualTo(PlayerId));
    }

    [Test]
    public void Test_ResolveRegisteredCaller_GuestToken_ThrowsGuestNotAllowedFault()
    {
        _sessionToken = _sessionTokens.Issue(PlayerIdentity.CreateGuest(GuestPlayerId, GuestUsername));

        Assert.That(
            ResolveArrangedRegisteredCaller,
            ThrowsFaultWithCode(GameServiceFaultCode.GuestNotAllowed));
    }

    [Test]
    public void Test_ResolveRegisteredCaller_UnknownToken_ThrowsSessionNotFoundFault()
    {
        _sessionToken = UnknownToken;

        Assert.That(
            ResolveArrangedRegisteredCaller,
            ThrowsFaultWithCode(GameServiceFaultCode.SessionNotFound));
    }

    private static IResolveConstraint ThrowsFaultWithCode(GameServiceFaultCode faultCode)
    {
        IResolveConstraint faultConstraint = Throws.TypeOf<FaultException<GameServiceFault>>()
            .With.Property(FaultDetailPropertyName)
            .With.Property(FaultCodePropertyName)
            .EqualTo(faultCode);

        return faultConstraint;
    }

    private void ResolveArrangedSignedInCaller()
    {
        TokenCallerResolver.ResolveSignedInCaller(_sessionTokens, _sessionToken);
    }

    private void ResolveArrangedRegisteredCaller()
    {
        TokenCallerResolver.ResolveRegisteredCaller(_sessionTokens, _sessionToken);
    }

    private void MoveClockTo(int minutesAfterIssue)
    {
        _clock.UtcNow.Returns(_issuedAt.AddMinutes(minutesAfterIssue));
    }
}
