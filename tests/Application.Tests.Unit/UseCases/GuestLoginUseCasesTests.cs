using System;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.UseCases;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace DominoTrainGame.Server.Application.Tests.Unit.UseCases;

[TestFixture]
public sealed class GuestLoginUseCasesTests
{
    private const int GuestPlayerId = 21;
    private const int SecondGuestPlayerId = 22;
    private const int ExpectedCallCount = 1;
    private const int RetryCallCount = 2;
    private const int MaximumUsernameAttempts = 5;
    private const string FirstGuestUsername = "Invitado111111";
    private const string SecondGuestUsername = "Invitado222222";
    private const string DatabaseErrorMessage = "The database is unavailable.";

    private IPlayerRepository _playerRepository;
    private IGuestUsernameGenerator _guestUsernameGenerator;
    private GuestLoginUseCases _guestLoginUseCases;

    [SetUp]
    public void SetUp()
    {
        _playerRepository = Substitute.For<IPlayerRepository>();
        _guestUsernameGenerator = Substitute.For<IGuestUsernameGenerator>();

        _guestUsernameGenerator.Generate().Returns(FirstGuestUsername, SecondGuestUsername);
        _playerRepository.CreateGuest(Arg.Any<string>()).Returns(PlayerIdentity.Empty);
        _playerRepository.CreateGuest(FirstGuestUsername)
            .Returns(new PlayerIdentity(GuestPlayerId, FirstGuestUsername));

        CreateGuestLoginUseCases();
    }

    [Test]
    public void Test_Constructor_NullPlayerRepository_ThrowsArgumentNullException()
    {
        _playerRepository = null;

        Assert.Throws<ArgumentNullException>(CreateGuestLoginUseCases);
    }

    [Test]
    public void Test_Constructor_NullGuestUsernameGenerator_ThrowsArgumentNullException()
    {
        _guestUsernameGenerator = null;

        Assert.Throws<ArgumentNullException>(CreateGuestLoginUseCases);
    }

    [Test]
    public void Test_LogInAsGuest_FreeUsername_ReturnsGuestPlayerId()
    {
        PlayerIdentity guest = _guestLoginUseCases.LogInAsGuest();

        Assert.That(guest.PlayerId, Is.EqualTo(GuestPlayerId));
    }

    [Test]
    public void Test_LogInAsGuest_FreeUsername_ReturnsGeneratedUsername()
    {
        PlayerIdentity guest = _guestLoginUseCases.LogInAsGuest();

        Assert.That(guest.Username, Is.EqualTo(FirstGuestUsername));
    }

    [Test]
    public void Test_LogInAsGuest_FreeUsername_GeneratesUsernameOnce()
    {
        _guestLoginUseCases.LogInAsGuest();

        _guestUsernameGenerator.Received(ExpectedCallCount).Generate();
    }

    [Test]
    public void Test_LogInAsGuest_FirstUsernameTaken_ReturnsGuestWithSecondUsername()
    {
        _playerRepository.CreateGuest(FirstGuestUsername).Returns(PlayerIdentity.Empty);
        _playerRepository.CreateGuest(SecondGuestUsername)
            .Returns(new PlayerIdentity(SecondGuestPlayerId, SecondGuestUsername));

        PlayerIdentity guest = _guestLoginUseCases.LogInAsGuest();

        Assert.That(guest.PlayerId, Is.EqualTo(SecondGuestPlayerId));
    }

    [Test]
    public void Test_LogInAsGuest_FirstUsernameTaken_GeneratesUsernameTwice()
    {
        _playerRepository.CreateGuest(FirstGuestUsername).Returns(PlayerIdentity.Empty);
        _playerRepository.CreateGuest(SecondGuestUsername)
            .Returns(new PlayerIdentity(SecondGuestPlayerId, SecondGuestUsername));

        _guestLoginUseCases.LogInAsGuest();

        _guestUsernameGenerator.Received(RetryCallCount).Generate();
    }

    [Test]
    public void Test_LogInAsGuest_AllUsernamesTaken_ReturnsEmptyIdentity()
    {
        _playerRepository.CreateGuest(FirstGuestUsername).Returns(PlayerIdentity.Empty);

        PlayerIdentity guest = _guestLoginUseCases.LogInAsGuest();

        Assert.That(guest.IsEmpty, Is.True);
    }

    [Test]
    public void Test_LogInAsGuest_AllUsernamesTaken_StopsAfterMaximumAttempts()
    {
        _playerRepository.CreateGuest(FirstGuestUsername).Returns(PlayerIdentity.Empty);

        _guestLoginUseCases.LogInAsGuest();

        _guestUsernameGenerator.Received(MaximumUsernameAttempts).Generate();
    }

    [Test]
    public void Test_LogInAsGuest_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _playerRepository.CreateGuest(FirstGuestUsername)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(LogInAsGuest);
    }

    private void CreateGuestLoginUseCases()
    {
        _guestLoginUseCases = new GuestLoginUseCases(_playerRepository, _guestUsernameGenerator);
    }

    private void LogInAsGuest()
    {
        _guestLoginUseCases.LogInAsGuest();
    }
}
