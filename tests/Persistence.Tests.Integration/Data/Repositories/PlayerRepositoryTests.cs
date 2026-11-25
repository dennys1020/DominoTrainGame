using System;
using System.Linq;
using System.Transactions;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Persistence.Data;
using DominoTrainGame.Server.Persistence.Data.Repositories;
using NUnit.Framework;

namespace DominoTrainGame.Server.Persistence.Tests.Integration.Data.Repositories;

[TestFixture]
public sealed class PlayerRepositoryTests
{
    private const int ExpectedProfileCount = 1;
    private const int NoProfiles = 0;
    private const string Username = "samuel";
    private const string Email = "samuel@uv.mx";
    private const string PasswordHash = "stored-hash";
    private const string OtherUsername = "denisse";
    private const string OtherEmail = "denisse@uv.mx";
    private const string GuestUsername = "Guest123456";
    private const string UnknownIdentifier = "nobody";
    private const string EmptyText = "";
    private const string WhitespaceText = "   ";

    private TransactionScope _transaction;
    private DatabaseExecutor _databaseExecutor;
    private PlayerRepository _playerRepository;
    private PlayerRegistration _registration;
    private string _guestUsername;

    [SetUp]
    public void SetUp()
    {
        _transaction = new TransactionScope();
        _databaseExecutor = new DatabaseExecutor();
        _playerRepository = new PlayerRepository(_databaseExecutor);
        _registration = new PlayerRegistration(Username, Email, PasswordHash);
        _guestUsername = GuestUsername;
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

        Assert.Throws<ArgumentNullException>(CreatePlayerRepository);
    }

    [Test]
    public void Test_CreatePlayer_NewPlayer_ReturnsTrue()
    {
        bool isRegistered = _playerRepository.CreatePlayer(_registration);

        Assert.That(isRegistered, Is.True);
    }

    [Test]
    public void Test_CreatePlayer_NewPlayer_CreatesProfile()
    {
        _playerRepository.CreatePlayer(_registration);

        int profileCount = _databaseExecutor.Execute(CountProfilesByUsername, Username);
        Assert.That(profileCount, Is.EqualTo(ExpectedProfileCount));
    }

    [Test]
    public void Test_CreatePlayer_UsernameTaken_ReturnsFalse()
    {
        _playerRepository.CreatePlayer(_registration);
        _registration = new PlayerRegistration(Username, OtherEmail, PasswordHash);

        bool isRegistered = _playerRepository.CreatePlayer(_registration);

        Assert.That(isRegistered, Is.False);
    }

    [Test]
    public void Test_CreatePlayer_EmailTaken_ReturnsFalse()
    {
        _playerRepository.CreatePlayer(_registration);
        _registration = new PlayerRegistration(OtherUsername, Email, PasswordHash);

        bool isRegistered = _playerRepository.CreatePlayer(_registration);

        Assert.That(isRegistered, Is.False);
    }

    [Test]
    public void Test_CreatePlayer_NullRegistration_ThrowsArgumentNullException()
    {
        _registration = null;

        Assert.Throws<ArgumentNullException>(CreateArrangedPlayer);
    }

    [Test]
    public void Test_FindCredentials_RegisteredUsername_ReturnsUsername()
    {
        _playerRepository.CreatePlayer(_registration);

        PlayerCredentials credentials = _playerRepository.FindCredentials(Username);

        Assert.That(credentials.Username, Is.EqualTo(Username));
    }

    [Test]
    public void Test_FindCredentials_RegisteredUsername_ReturnsStoredPasswordHash()
    {
        _playerRepository.CreatePlayer(_registration);

        PlayerCredentials credentials = _playerRepository.FindCredentials(Username);

        Assert.That(credentials.PasswordHash, Is.EqualTo(PasswordHash));
    }

    [Test]
    public void Test_FindCredentials_RegisteredEmail_ReturnsUsername()
    {
        _playerRepository.CreatePlayer(_registration);

        PlayerCredentials credentials = _playerRepository.FindCredentials(Email);

        Assert.That(credentials.Username, Is.EqualTo(Username));
    }

    [Test]
    public void Test_FindCredentials_UnknownIdentifier_ReturnsEmptyCredentials()
    {
        PlayerCredentials credentials = _playerRepository.FindCredentials(UnknownIdentifier);

        Assert.That(credentials.IsEmpty, Is.True);
    }

    [Test]
    public void Test_FindCredentials_GuestUsername_ReturnsEmptyCredentials()
    {
        _playerRepository.CreateGuest(_guestUsername);

        PlayerCredentials credentials = _playerRepository.FindCredentials(GuestUsername);

        Assert.That(credentials.IsEmpty, Is.True);
    }

    [Test]
    public void Test_CreateGuest_FreeUsername_ReturnsGuestIdentity()
    {
        PlayerIdentity guest = _playerRepository.CreateGuest(_guestUsername);

        Assert.That(guest.IsGuest, Is.True);
    }

    [Test]
    public void Test_CreateGuest_FreeUsername_ReturnsUsername()
    {
        PlayerIdentity guest = _playerRepository.CreateGuest(_guestUsername);

        Assert.That(guest.Username, Is.EqualTo(GuestUsername));
    }

    [Test]
    public void Test_CreateGuest_FreeUsername_DoesNotCreateProfile()
    {
        _playerRepository.CreateGuest(_guestUsername);

        int profileCount = _databaseExecutor.Execute(CountProfilesByUsername, GuestUsername);
        Assert.That(profileCount, Is.EqualTo(NoProfiles));
    }

    [Test]
    public void Test_CreateGuest_UsernameTaken_ReturnsEmptyIdentity()
    {
        _playerRepository.CreatePlayer(_registration);
        _guestUsername = Username;

        PlayerIdentity guest = _playerRepository.CreateGuest(_guestUsername);

        Assert.That(guest.IsEmpty, Is.True);
    }

    [TestCase(EmptyText)]
    [TestCase(WhitespaceText)]
    [TestCase(null)]
    public void Test_CreateGuest_BlankUsername_ThrowsArgumentException(string blankUsername)
    {
        _guestUsername = blankUsername;

        Assert.Throws<ArgumentException>(CreateArrangedGuest);
    }

    [Test]
    public void Test_IsRegistrationTaken_UsedUsername_ReturnsTrue()
    {
        _playerRepository.CreatePlayer(_registration);

        bool isTaken = _playerRepository.IsRegistrationTaken(Username, OtherEmail);

        Assert.That(isTaken, Is.True);
    }

    [Test]
    public void Test_IsRegistrationTaken_UsedEmail_ReturnsTrue()
    {
        _playerRepository.CreatePlayer(_registration);

        bool isTaken = _playerRepository.IsRegistrationTaken(OtherUsername, Email);

        Assert.That(isTaken, Is.True);
    }

    [Test]
    public void Test_IsRegistrationTaken_FreeUsernameAndEmail_ReturnsFalse()
    {
        _playerRepository.CreatePlayer(_registration);

        bool isTaken = _playerRepository.IsRegistrationTaken(OtherUsername, OtherEmail);

        Assert.That(isTaken, Is.False);
    }

    private static int CountProfilesByUsername(DominoGameDBEntities context, string username)
    {
        int profileCount = (from profile in context.PlayerProfiles
                            where profile.Player.Username == username
                            select profile).Count();
        return profileCount;
    }

    private void CreatePlayerRepository()
    {
        _playerRepository = new PlayerRepository(_databaseExecutor);
    }

    private void CreateArrangedPlayer()
    {
        _playerRepository.CreatePlayer(_registration);
    }

    private void CreateArrangedGuest()
    {
        _playerRepository.CreateGuest(_guestUsername);
    }
}
