using System;
using System.Transactions;
using NUnit.Framework;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Persistence.Data;
using DominoTrainGame.Server.Persistence.Data.Repositories;
using DominoTrainGame.Server.Persistence.Time;

namespace DominoTrainGame.Server.Persistence.Tests.Integration.Data.Repositories;

[TestFixture]
public sealed class ProfileRepositoryTests
{
    private const string Username = "denisse";
    private const string Email = "denisse@uv.mx";
    private const string OtherUsername = "samuel";
    private const string OtherEmail = "samuel@uv.mx";
    private const string PasswordHash = "stored-hash";
    private const string GuestUsername = "Guest654321";
    private const string UnknownUsername = "nobody";
    private const string NewUsername = "denisse_uv";
    private const string Biography = "Me gusta el domino.";
    private const string InstagramUsername = "denisse.uv";
    private const string FacebookUsername = "denisse.juarez";
    private const string TwitterUsername = "denisse_x";
    private const string AvatarFileName = "0123456789abcdef0123456789abcdef.png";

    private TransactionScope _transaction;
    private DatabaseExecutor _databaseExecutor;
    private PlayerRepository _playerRepository;
    private ProfileRepository _profileRepository;

    [SetUp]
    public void SetUp()
    {
        _transaction = new TransactionScope();
        _databaseExecutor = new DatabaseExecutor();
        _playerRepository = new PlayerRepository(_databaseExecutor, new SystemClock());
        CreateProfileRepository();
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

        Assert.Throws<ArgumentNullException>(CreateProfileRepository);
    }

    [Test]
    public void Test_FindProfileByUsername_RegisteredPlayer_ReturnsUsername()
    {
        RegisterPlayer(Username, Email);

        PlayerProfileSummary profile = _profileRepository.FindProfileByUsername(Username);

        Assert.That(profile.Player.Username, Is.EqualTo(Username));
    }

    [Test]
    public void Test_FindProfileByUsername_UnknownUsername_ReturnsEmptyProfile()
    {
        PlayerProfileSummary profile = _profileRepository.FindProfileByUsername(UnknownUsername);

        Assert.That(profile.IsEmpty, Is.True);
    }

    [Test]
    public void Test_FindProfileByUsername_GuestPlayer_ReturnsEmptyProfile()
    {
        _playerRepository.CreateGuest(GuestUsername);

        PlayerProfileSummary profile = _profileRepository.FindProfileByUsername(GuestUsername);

        Assert.That(profile.IsEmpty, Is.True);
    }

    [Test]
    public void Test_FindProfileByPlayerId_RegisteredPlayer_ReturnsUsername()
    {
        int playerId = RegisterPlayer(Username, Email);

        PlayerProfileSummary profile = _profileRepository.FindProfileByPlayerId(playerId);

        Assert.That(profile.Player.Username, Is.EqualTo(Username));
    }

    [Test]
    public void Test_UpdateBiography_RegisteredPlayer_StoresBiography()
    {
        int playerId = RegisterPlayer(Username, Email);
        _profileRepository.UpdateBiography(playerId, Biography);

        PlayerProfileSummary profile = _profileRepository.FindProfileByPlayerId(playerId);

        Assert.That(profile.Details.Biography, Is.EqualTo(Biography));
    }

    [Test]
    public void Test_UpdateBiography_GuestPlayer_ReturnsFalse()
    {
        int guestId = _playerRepository.CreateGuest(GuestUsername).PlayerId;

        bool isUpdated = _profileRepository.UpdateBiography(guestId, Biography);

        Assert.That(isUpdated, Is.False);
    }

    [Test]
    public void Test_UpdateBiography_SameBiography_ReturnsTrue()
    {
        int playerId = RegisterPlayer(Username, Email);
        _profileRepository.UpdateBiography(playerId, Biography);

        bool isUpdated = _profileRepository.UpdateBiography(playerId, Biography);

        Assert.That(isUpdated, Is.True);
    }

    [Test]
    public void Test_UpdateSocialLinks_RegisteredPlayer_StoresTwitterUsername()
    {
        int playerId = RegisterPlayer(Username, Email);
        SocialLinks socialLinks = new SocialLinks(InstagramUsername, FacebookUsername, TwitterUsername);
        _profileRepository.UpdateSocialLinks(playerId, socialLinks);

        PlayerProfileSummary profile = _profileRepository.FindProfileByPlayerId(playerId);

        Assert.That(profile.SocialLinks.TwitterUsername, Is.EqualTo(TwitterUsername));
    }

    [Test]
    public void Test_UpdateSocialLinks_NullLinks_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(UpdateSocialLinksWithNullLinks);
    }

    [Test]
    public void Test_UpdateAvatarFileName_RegisteredPlayer_StoresFileName()
    {
        int playerId = RegisterPlayer(Username, Email);
        _profileRepository.UpdateAvatarFileName(playerId, AvatarFileName);

        PlayerProfileSummary profile = _profileRepository.FindProfileByPlayerId(playerId);

        Assert.That(profile.Details.AvatarFileName, Is.EqualTo(AvatarFileName));
    }

    [Test]
    public void Test_ChangeUsername_FreeUsername_ReturnsSuccess()
    {
        int playerId = RegisterPlayer(Username, Email);

        ChangeUsernameStatus status = _profileRepository.ChangeUsername(playerId, NewUsername);

        Assert.That(status, Is.EqualTo(ChangeUsernameStatus.Success));
    }

    [Test]
    public void Test_ChangeUsername_FreeUsername_StoresNewUsername()
    {
        int playerId = RegisterPlayer(Username, Email);
        _profileRepository.ChangeUsername(playerId, NewUsername);

        PlayerProfileSummary profile = _profileRepository.FindProfileByPlayerId(playerId);

        Assert.That(profile.Player.Username, Is.EqualTo(NewUsername));
    }

    [Test]
    public void Test_ChangeUsername_CurrentUsername_ReturnsSuccess()
    {
        int playerId = RegisterPlayer(Username, Email);

        ChangeUsernameStatus status = _profileRepository.ChangeUsername(playerId, Username);

        Assert.That(status, Is.EqualTo(ChangeUsernameStatus.Success));
    }

    [Test]
    public void Test_ChangeUsername_UsernameOfOtherPlayer_ReturnsUsernameTaken()
    {
        int playerId = RegisterPlayer(Username, Email);
        RegisterPlayer(OtherUsername, OtherEmail);

        ChangeUsernameStatus status = _profileRepository.ChangeUsername(playerId, OtherUsername);

        Assert.That(status, Is.EqualTo(ChangeUsernameStatus.UsernameTaken));
    }

    [Test]
    public void Test_ChangeUsername_GuestPlayer_ReturnsGuestNotAllowed()
    {
        int guestId = _playerRepository.CreateGuest(GuestUsername).PlayerId;

        ChangeUsernameStatus status = _profileRepository.ChangeUsername(guestId, NewUsername);

        Assert.That(status, Is.EqualTo(ChangeUsernameStatus.GuestNotAllowed));
    }

    private int RegisterPlayer(string username, string email)
    {
        _playerRepository.CreatePlayer(new PlayerRegistration(username, email, PasswordHash));
        int playerId = _profileRepository.FindProfileByUsername(username).Player.PlayerId;

        return playerId;
    }

    private void CreateProfileRepository()
    {
        _profileRepository = new ProfileRepository(_databaseExecutor);
    }

    private void UpdateSocialLinksWithNullLinks()
    {
        _profileRepository.UpdateSocialLinks(PlayerIdentity.EmptyPlayerId, null);
    }
}
