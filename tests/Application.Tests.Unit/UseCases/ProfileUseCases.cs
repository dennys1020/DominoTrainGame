using System;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Application.UseCases.Validation;

namespace DominoTrainGame.Server.Application.Tests.Unit.UseCases;

[TestFixture]
public sealed class ProfileUseCasesTests
{
    private const int PlayerId = 7;
    private const int GuestPlayerId = 9;
    private const int ExpectedCallCount = 1;
    private const int GamesPlayed = 12;
    private const int Wins = 5;
    private const int TotalPoints = 340;
    private const int MemberSinceYear = 2026;
    private const int MemberSinceMonth = 11;
    private const int MemberSinceDay = 21;
    private const int TooLongBiographyLength = ProfileInputValidator.MaximumBiographyLength + 1;
    private const int TooLongUsernameLength = PlayerInputValidator.MaximumUsernameLength + 1;
    private const int TooLongInstagramUsernameLength = ProfileInputValidator.MaximumInstagramUsernameLength + 1;
    private const int TooLargeAvatarImageLength = AvatarImageValidator.MaximumAvatarImageBytes + 1;
    private const char FillerCharacter = 'a';
    private const string Username = "denisse";
    private const string PaddedUsername = "  denisse  ";
    private const string UnknownUsername = "nobody";
    private const string NewUsername = "denisse_uv";
    private const string PaddedNewUsername = "  denisse_uv  ";
    private const string ShortUsername = "ab";
    private const string InvalidCharactersUsername = "denisse uv!";
    private const string Biography = "Me gusta el domino.";
    private const string PaddedBiography = "  Me gusta el domino.  ";
    private const string InstagramUsername = "denisse.uv";
    private const string FacebookUsername = "denisse.juarez";
    private const string TwitterUsername = "denisse_x";
    private const string PrefixedTwitterUsername = "@denisse_x";
    private const string InvalidInstagramUsername = "denisse uv";
    private const string InvalidFacebookUsername = "abc";
    private const string InvalidTwitterUsername = "denisse_twitter_x";
    private const string CurrentAvatarFileName = "0123456789abcdef0123456789abcdef.png";
    private const string NewAvatarFileName = "fedcba9876543210fedcba9876543210.png";
    private const string PngFileExtension = ".png";
    private const string JpgFileExtension = ".jpg";
    private const string DatabaseErrorMessage = "The database is unavailable.";
    private const string StorageErrorMessage = "The avatar folder is unavailable.";
    private const string DataIntegrityErrorMessage = "The changes violate a database constraint.";

    private static readonly byte[] _pngImageBytes;
    private static readonly byte[] _jpgImageBytes;
    private static readonly byte[] _gifImageBytes;
    private static readonly byte[] _storedAvatarImage;
    private static readonly DateTime _memberSince;

    private IProfileRepository _profileRepository;
    private IMatchResultRepository _matchResultRepository;
    private IAvatarStorage _avatarStorage;
    private ProfileUseCases _profileUseCases;
    private PlayerProfileSummary _existingProfile;
    private PlayerProfileStatistics _statistics;
    private PlayerProfileSummary _profile;
    private int _playerId;
    private string _username;
    private string _biography;
    private string _newUsername;
    private SocialLinks _socialLinks;
    private SocialLinks _storedSocialLinks;
    private byte[] _imageBytes;
    private Exception _caughtException;

    static ProfileUseCasesTests()
    {
        _pngImageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 };
        _jpgImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00 };
        _gifImageBytes = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };
        _storedAvatarImage = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01 };
        _memberSince = DateTime.SpecifyKind(
            new DateTime(MemberSinceYear, MemberSinceMonth, MemberSinceDay),
            DateTimeKind.Utc);
    }

    [SetUp]
    public void SetUp()
    {
        _profileRepository = Substitute.For<IProfileRepository>();
        _matchResultRepository = Substitute.For<IMatchResultRepository>();
        _avatarStorage = Substitute.For<IAvatarStorage>();
        _existingProfile = CreateProfile(CurrentAvatarFileName);
        _statistics = new PlayerProfileStatistics(GamesPlayed, Wins, TotalPoints);

        _profileRepository.FindProfileByUsername(Arg.Any<string>()).Returns(PlayerProfileSummary.Empty);
        _profileRepository.FindProfileByUsername(Username).Returns(_existingProfile);
        _profileRepository.FindProfileByPlayerId(Arg.Any<int>()).Returns(PlayerProfileSummary.Empty);
        _profileRepository.FindProfileByPlayerId(PlayerId).Returns(_existingProfile);
        _profileRepository.UpdateBiography(PlayerId, Arg.Any<string>()).Returns(true);
        _profileRepository.UpdateSocialLinks(PlayerId, Arg.Any<SocialLinks>()).Returns(true);
        _profileRepository.UpdateAvatarFileName(PlayerId, Arg.Any<string>()).Returns(true);
        _profileRepository.ChangeUsername(PlayerId, Arg.Any<string>()).Returns(ChangeUsernameStatus.Success);
        _profileRepository.ChangeUsername(GuestPlayerId, Arg.Any<string>())
            .Returns(ChangeUsernameStatus.GuestNotAllowed);
        _matchResultRepository.GetProfileStatisticsByPlayerId(PlayerId).Returns(_statistics);
        _avatarStorage.SaveAvatarImage(Arg.Any<byte[]>(), Arg.Any<string>()).Returns(NewAvatarFileName);
        _avatarStorage.LoadAvatarImage(CurrentAvatarFileName).Returns(_storedAvatarImage);

        _playerId = PlayerId;
        _username = Username;
        _biography = Biography;
        _newUsername = NewUsername;
        _socialLinks = new SocialLinks(InstagramUsername, FacebookUsername, TwitterUsername);
        _imageBytes = _pngImageBytes;
        _profile = _existingProfile;
        _caughtException = null;
        CreateProfileUseCases();
    }

    [Test]
    public void Test_Constructor_NullProfileRepository_ThrowsArgumentNullException()
    {
        _profileRepository = null;

        Assert.Throws<ArgumentNullException>(CreateProfileUseCases);
    }

    [Test]
    public void Test_Constructor_NullMatchResultRepository_ThrowsArgumentNullException()
    {
        _matchResultRepository = null;

        Assert.Throws<ArgumentNullException>(CreateProfileUseCases);
    }

    [Test]
    public void Test_Constructor_NullAvatarStorage_ThrowsArgumentNullException()
    {
        _avatarStorage = null;

        Assert.Throws<ArgumentNullException>(CreateProfileUseCases);
    }

    [Test]
    public void Test_FindProfileByUsername_ExistingUsername_ReturnsStoredProfile()
    {
        PlayerProfileSummary profile = _profileUseCases.FindProfileByUsername(Username);

        Assert.That(profile, Is.SameAs(_existingProfile));
    }

    [Test]
    public void Test_FindProfileByUsername_PaddedUsername_SearchesTrimmedUsername()
    {
        _profileUseCases.FindProfileByUsername(PaddedUsername);

        _profileRepository.Received(ExpectedCallCount).FindProfileByUsername(Username);
    }

    [Test]
    public void Test_FindProfileByUsername_UnknownUsername_ReturnsEmptyProfile()
    {
        PlayerProfileSummary profile = _profileUseCases.FindProfileByUsername(UnknownUsername);

        Assert.That(profile.IsEmpty, Is.True);
    }

    [Test]
    public void Test_FindProfileByUsername_EmptyUsername_ThrowsArgumentException()
    {
        _username = string.Empty;

        Assert.Throws<ArgumentException>(FindProfileWithArrangedUsername);
    }

    [Test]
    public void Test_FindProfileByUsername_TooLongUsername_ThrowsArgumentException()
    {
        _username = new string(FillerCharacter, TooLongUsernameLength);

        Assert.Throws<ArgumentException>(FindProfileWithArrangedUsername);
    }

    [Test]
    public void Test_FindProfileByUsername_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _profileRepository.FindProfileByUsername(Username)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(FindProfileWithArrangedUsername);
    }

    [Test]
    public void Test_GetProfileStatistics_RegisteredPlayer_ReturnsStoredStatistics()
    {
        PlayerProfileStatistics statistics = _profileUseCases.GetProfileStatistics(PlayerId);

        Assert.That(statistics, Is.SameAs(_statistics));
    }

    [Test]
    public void Test_GetProfileStatistics_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _matchResultRepository.GetProfileStatisticsByPlayerId(PlayerId)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(GetStatisticsOfArrangedPlayer);
    }

    [Test]
    public void Test_LoadProfileAvatarImage_ProfileWithAvatar_ReturnsStoredImage()
    {
        byte[] avatarImage = _profileUseCases.LoadProfileAvatarImage(_existingProfile);

        Assert.That(avatarImage, Is.SameAs(_storedAvatarImage));
    }

    [Test]
    public void Test_LoadProfileAvatarImage_ProfileWithoutAvatar_ReturnsEmptyImage()
    {
        PlayerProfileSummary profileWithoutAvatar = CreateProfile(string.Empty);

        byte[] avatarImage = _profileUseCases.LoadProfileAvatarImage(profileWithoutAvatar);

        Assert.That(avatarImage, Is.Empty);
    }

    [Test]
    public void Test_LoadProfileAvatarImage_ProfileWithoutAvatar_DoesNotLoadStorage()
    {
        PlayerProfileSummary profileWithoutAvatar = CreateProfile(string.Empty);

        _profileUseCases.LoadProfileAvatarImage(profileWithoutAvatar);

        _avatarStorage.DidNotReceive().LoadAvatarImage(Arg.Any<string>());
    }

    [Test]
    public void Test_LoadProfileAvatarImage_StorageUnavailable_ReturnsEmptyImage()
    {
        _avatarStorage.LoadAvatarImage(CurrentAvatarFileName)
            .Throws(new AvatarStorageUnavailableException(StorageErrorMessage));

        byte[] avatarImage = _profileUseCases.LoadProfileAvatarImage(_existingProfile);

        Assert.That(avatarImage, Is.Empty);
    }

    [Test]
    public void Test_LoadProfileAvatarImage_NullProfile_ThrowsArgumentNullException()
    {
        _profile = null;

        Assert.Throws<ArgumentNullException>(LoadAvatarImageOfArrangedProfile);
    }

    [Test]
    public void Test_UpdateProfileBiography_RegisteredPlayer_ReturnsTrue()
    {
        bool isUpdated = _profileUseCases.UpdateProfileBiography(PlayerId, Biography);

        Assert.That(isUpdated, Is.True);
    }

    [Test]
    public void Test_UpdateProfileBiography_PaddedBiography_StoresTrimmedBiography()
    {
        _profileUseCases.UpdateProfileBiography(PlayerId, PaddedBiography);

        _profileRepository.Received(ExpectedCallCount).UpdateBiography(PlayerId, Biography);
    }

    [Test]
    public void Test_UpdateProfileBiography_NullBiography_StoresEmptyBiography()
    {
        _profileUseCases.UpdateProfileBiography(PlayerId, null);

        _profileRepository.Received(ExpectedCallCount).UpdateBiography(PlayerId, string.Empty);
    }

    [Test]
    public void Test_UpdateProfileBiography_MaximumLengthBiography_ReturnsTrue()
    {
        string longestBiography = new string(FillerCharacter, ProfileInputValidator.MaximumBiographyLength);

        bool isUpdated = _profileUseCases.UpdateProfileBiography(PlayerId, longestBiography);

        Assert.That(isUpdated, Is.True);
    }

    [Test]
    public void Test_UpdateProfileBiography_TooLongBiography_ThrowsArgumentException()
    {
        _biography = new string(FillerCharacter, TooLongBiographyLength);

        Assert.Throws<ArgumentException>(UpdateBiographyWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileBiography_GuestPlayer_ReturnsFalse()
    {
        bool isUpdated = _profileUseCases.UpdateProfileBiography(GuestPlayerId, Biography);

        Assert.That(isUpdated, Is.False);
    }

    [Test]
    public void Test_UpdateProfileBiography_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _profileRepository.UpdateBiography(PlayerId, Biography)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(UpdateBiographyWithArrangedData);
    }

    [Test]
    public void Test_ChangeProfileUsername_AvailableUsername_ReturnsSuccess()
    {
        ChangeUsernameStatus status = _profileUseCases.ChangeProfileUsername(PlayerId, NewUsername);

        Assert.That(status, Is.EqualTo(ChangeUsernameStatus.Success));
    }

    [Test]
    public void Test_ChangeProfileUsername_PaddedUsername_StoresTrimmedUsername()
    {
        _profileUseCases.ChangeProfileUsername(PlayerId, PaddedNewUsername);

        _profileRepository.Received(ExpectedCallCount).ChangeUsername(PlayerId, NewUsername);
    }

    [Test]
    public void Test_ChangeProfileUsername_TakenUsername_ReturnsUsernameTaken()
    {
        _profileRepository.ChangeUsername(PlayerId, NewUsername).Returns(ChangeUsernameStatus.UsernameTaken);

        ChangeUsernameStatus status = _profileUseCases.ChangeProfileUsername(PlayerId, NewUsername);

        Assert.That(status, Is.EqualTo(ChangeUsernameStatus.UsernameTaken));
    }

    [Test]
    public void Test_ChangeProfileUsername_GuestPlayer_ReturnsGuestNotAllowed()
    {
        ChangeUsernameStatus status = _profileUseCases.ChangeProfileUsername(GuestPlayerId, NewUsername);

        Assert.That(status, Is.EqualTo(ChangeUsernameStatus.GuestNotAllowed));
    }

    [TestCase(ShortUsername)]
    [TestCase(InvalidCharactersUsername)]
    [TestCase(null)]
    public void Test_ChangeProfileUsername_InvalidUsername_ThrowsArgumentException(string invalidUsername)
    {
        _newUsername = invalidUsername;

        Assert.Throws<ArgumentException>(ChangeUsernameWithArrangedData);
    }

    [Test]
    public void Test_ChangeProfileUsername_TooLongUsername_ThrowsArgumentException()
    {
        _newUsername = new string(FillerCharacter, TooLongUsernameLength);

        Assert.Throws<ArgumentException>(ChangeUsernameWithArrangedData);
    }

    [Test]
    public void Test_ChangeProfileUsername_InvalidUsername_DoesNotCallRepository()
    {
        _newUsername = InvalidCharactersUsername;
        CatchExpectedException<ArgumentException>(ChangeUsernameWithArrangedData);

        _profileRepository.DidNotReceive().ChangeUsername(Arg.Any<int>(), Arg.Any<string>());
    }

    [Test]
    public void Test_ChangeProfileUsername_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _profileRepository.ChangeUsername(PlayerId, NewUsername)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(ChangeUsernameWithArrangedData);
    }

    [Test]
    public void Test_ChangeProfileUsername_UniqueConstraintViolated_ReturnsUsernameTaken()
    {
        _profileRepository.ChangeUsername(PlayerId, NewUsername)
            .Throws(new DataIntegrityException(DataIntegrityErrorMessage));

        ChangeUsernameStatus status = _profileUseCases.ChangeProfileUsername(PlayerId, NewUsername);

        Assert.That(status, Is.EqualTo(ChangeUsernameStatus.UsernameTaken));
    }

    [Test]
    public void Test_UpdateProfileSocialLinks_ValidLinks_ReturnsTrue()
    {
        bool isUpdated = _profileUseCases.UpdateProfileSocialLinks(PlayerId, _socialLinks);

        Assert.That(isUpdated, Is.True);
    }

    [Test]
    public void Test_UpdateProfileSocialLinks_EmptyLinks_ReturnsTrue()
    {
        bool isUpdated = _profileUseCases.UpdateProfileSocialLinks(PlayerId, SocialLinks.Empty);

        Assert.That(isUpdated, Is.True);
    }

    [Test]
    public void Test_UpdateProfileSocialLinks_PrefixedUsername_StoresUsernameWithoutPrefix()
    {
        SocialLinks prefixedLinks = new SocialLinks(InstagramUsername, FacebookUsername, PrefixedTwitterUsername);
        _profileRepository.UpdateSocialLinks(PlayerId, Arg.Any<SocialLinks>()).Returns(CaptureStoredSocialLinks);

        _profileUseCases.UpdateProfileSocialLinks(PlayerId, prefixedLinks);

        Assert.That(_storedSocialLinks.TwitterUsername, Is.EqualTo(TwitterUsername));
    }

    [TestCase(InvalidInstagramUsername, FacebookUsername, TwitterUsername)]
    [TestCase(InstagramUsername, InvalidFacebookUsername, TwitterUsername)]
    [TestCase(InstagramUsername, FacebookUsername, InvalidTwitterUsername)]
    public void Test_UpdateProfileSocialLinks_InvalidSocialUsername_ThrowsArgumentException(
        string instagramUsername,
        string facebookUsername,
        string twitterUsername)
    {
        _socialLinks = new SocialLinks(instagramUsername, facebookUsername, twitterUsername);

        Assert.Throws<ArgumentException>(UpdateSocialLinksWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileSocialLinks_TooLongInstagramUsername_ThrowsArgumentException()
    {
        string tooLongInstagramUsername = new string(FillerCharacter, TooLongInstagramUsernameLength);
        _socialLinks = new SocialLinks(tooLongInstagramUsername, FacebookUsername, TwitterUsername);

        Assert.Throws<ArgumentException>(UpdateSocialLinksWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileSocialLinks_NullLinks_ThrowsArgumentNullException()
    {
        _socialLinks = null;

        Assert.Throws<ArgumentNullException>(UpdateSocialLinksWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileSocialLinks_GuestPlayer_ReturnsFalse()
    {
        bool isUpdated = _profileUseCases.UpdateProfileSocialLinks(GuestPlayerId, _socialLinks);

        Assert.That(isUpdated, Is.False);
    }

    [Test]
    public void Test_UpdateProfileSocialLinks_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _profileRepository.UpdateSocialLinks(PlayerId, Arg.Any<SocialLinks>())
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(UpdateSocialLinksWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileAvatar_PngImage_ReturnsTrue()
    {
        bool isUpdated = _profileUseCases.UpdateProfileAvatar(PlayerId, _pngImageBytes);

        Assert.That(isUpdated, Is.True);
    }

    [Test]
    public void Test_UpdateProfileAvatar_PngImage_SavesFileWithPngExtension()
    {
        _profileUseCases.UpdateProfileAvatar(PlayerId, _pngImageBytes);

        _avatarStorage.Received(ExpectedCallCount).SaveAvatarImage(_pngImageBytes, PngFileExtension);
    }

    [Test]
    public void Test_UpdateProfileAvatar_JpgImage_SavesFileWithJpgExtension()
    {
        _profileUseCases.UpdateProfileAvatar(PlayerId, _jpgImageBytes);

        _avatarStorage.Received(ExpectedCallCount).SaveAvatarImage(_jpgImageBytes, JpgFileExtension);
    }

    [Test]
    public void Test_UpdateProfileAvatar_ValidImage_StoresNewFileName()
    {
        _profileUseCases.UpdateProfileAvatar(PlayerId, _pngImageBytes);

        _profileRepository.Received(ExpectedCallCount).UpdateAvatarFileName(PlayerId, NewAvatarFileName);
    }

    [Test]
    public void Test_UpdateProfileAvatar_PreviousAvatar_DeletesPreviousFile()
    {
        _profileUseCases.UpdateProfileAvatar(PlayerId, _pngImageBytes);

        _avatarStorage.Received(ExpectedCallCount).DeleteAvatarImage(CurrentAvatarFileName);
    }

    [Test]
    public void Test_UpdateProfileAvatar_NoPreviousAvatar_DoesNotDeleteAnyFile()
    {
        _profileRepository.FindProfileByPlayerId(PlayerId).Returns(CreateProfile(string.Empty));

        _profileUseCases.UpdateProfileAvatar(PlayerId, _pngImageBytes);

        _avatarStorage.DidNotReceive().DeleteAvatarImage(Arg.Any<string>());
    }

    [Test]
    public void Test_UpdateProfileAvatar_PreviousFileCannotBeDeleted_ReturnsTrue()
    {
        _avatarStorage.When(DeleteCurrentAvatarImage)
            .Throw(new AvatarStorageUnavailableException(StorageErrorMessage));

        bool isUpdated = _profileUseCases.UpdateProfileAvatar(PlayerId, _pngImageBytes);

        Assert.That(isUpdated, Is.True);
    }

    [Test]
    public void Test_UpdateProfileAvatar_ProfileRowNotUpdated_DeletesNewFile()
    {
        _profileRepository.UpdateAvatarFileName(PlayerId, NewAvatarFileName).Returns(false);

        _profileUseCases.UpdateProfileAvatar(PlayerId, _pngImageBytes);

        _avatarStorage.Received(ExpectedCallCount).DeleteAvatarImage(NewAvatarFileName);
    }

    [Test]
    public void Test_UpdateProfileAvatar_ProfileRowNotUpdated_ReturnsFalse()
    {
        _profileRepository.UpdateAvatarFileName(PlayerId, NewAvatarFileName).Returns(false);

        bool isUpdated = _profileUseCases.UpdateProfileAvatar(PlayerId, _pngImageBytes);

        Assert.That(isUpdated, Is.False);
    }

    [Test]
    public void Test_UpdateProfileAvatar_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _profileRepository.UpdateAvatarFileName(PlayerId, NewAvatarFileName)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(UpdateAvatarWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileAvatar_DatabaseUnavailable_DeletesNewFile()
    {
        _profileRepository.UpdateAvatarFileName(PlayerId, NewAvatarFileName)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));
        CatchExpectedException<DatabaseUnavailableException>(UpdateAvatarWithArrangedData);

        _avatarStorage.Received(ExpectedCallCount).DeleteAvatarImage(NewAvatarFileName);
    }

    [Test]
    public void Test_UpdateProfileAvatar_DatabaseUnavailable_KeepsPreviousFile()
    {
        _profileRepository.UpdateAvatarFileName(PlayerId, NewAvatarFileName)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));
        CatchExpectedException<DatabaseUnavailableException>(UpdateAvatarWithArrangedData);

        _avatarStorage.DidNotReceive().DeleteAvatarImage(CurrentAvatarFileName);
    }

    [Test]
    public void Test_UpdateProfileAvatar_DataIntegrityViolation_ThrowsDataIntegrityException()
    {
        _profileRepository.UpdateAvatarFileName(PlayerId, NewAvatarFileName)
            .Throws(new DataIntegrityException(DataIntegrityErrorMessage));

        Assert.Throws<DataIntegrityException>(UpdateAvatarWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileAvatar_DataIntegrityViolation_DeletesNewFile()
    {
        _profileRepository.UpdateAvatarFileName(PlayerId, NewAvatarFileName)
            .Throws(new DataIntegrityException(DataIntegrityErrorMessage));
        CatchExpectedException<DataIntegrityException>(UpdateAvatarWithArrangedData);

        _avatarStorage.Received(ExpectedCallCount).DeleteAvatarImage(NewAvatarFileName);
    }

    [Test]
    public void Test_UpdateProfileAvatar_StorageUnavailable_ThrowsAvatarStorageUnavailableException()
    {
        _avatarStorage.SaveAvatarImage(Arg.Any<byte[]>(), Arg.Any<string>())
            .Throws(new AvatarStorageUnavailableException(StorageErrorMessage));

        Assert.Throws<AvatarStorageUnavailableException>(UpdateAvatarWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileAvatar_GuestPlayer_ReturnsFalse()
    {
        bool isUpdated = _profileUseCases.UpdateProfileAvatar(GuestPlayerId, _pngImageBytes);

        Assert.That(isUpdated, Is.False);
    }

    [Test]
    public void Test_UpdateProfileAvatar_GuestPlayer_DoesNotSaveFile()
    {
        _profileUseCases.UpdateProfileAvatar(GuestPlayerId, _pngImageBytes);

        _avatarStorage.DidNotReceive().SaveAvatarImage(Arg.Any<byte[]>(), Arg.Any<string>());
    }

    [Test]
    public void Test_UpdateProfileAvatar_NullImage_ThrowsArgumentException()
    {
        _imageBytes = null;

        Assert.Throws<ArgumentException>(UpdateAvatarWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileAvatar_EmptyImage_ThrowsArgumentException()
    {
        _imageBytes = Array.Empty<byte>();

        Assert.Throws<ArgumentException>(UpdateAvatarWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileAvatar_TooLargeImage_ThrowsArgumentException()
    {
        _imageBytes = CreateTooLargePngImage();

        Assert.Throws<ArgumentException>(UpdateAvatarWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileAvatar_UnsupportedFormat_ThrowsArgumentException()
    {
        _imageBytes = _gifImageBytes;

        Assert.Throws<ArgumentException>(UpdateAvatarWithArrangedData);
    }

    [Test]
    public void Test_UpdateProfileAvatar_UnsupportedFormat_DoesNotSaveFile()
    {
        _imageBytes = _gifImageBytes;
        CatchExpectedException<ArgumentException>(UpdateAvatarWithArrangedData);

        _avatarStorage.DidNotReceive().SaveAvatarImage(Arg.Any<byte[]>(), Arg.Any<string>());
    }

    private static PlayerProfileSummary CreateProfile(string avatarFileName)
    {
        PlayerIdentity player = new PlayerIdentity(PlayerId, Username);
        ProfileDetails details = new ProfileDetails(Biography, avatarFileName, _memberSince);
        SocialLinks socialLinks = new SocialLinks(InstagramUsername, FacebookUsername, TwitterUsername);
        PlayerProfileSummary profile = new PlayerProfileSummary(player, details, socialLinks);

        return profile;
    }

    private static byte[] CreateTooLargePngImage()
    {
        byte[] imageBytes = new byte[TooLargeAvatarImageLength];
        Array.Copy(_pngImageBytes, imageBytes, _pngImageBytes.Length);

        return imageBytes;
    }

    private static void DeleteCurrentAvatarImage(IAvatarStorage avatarStorage)
    {
        avatarStorage.DeleteAvatarImage(CurrentAvatarFileName);
    }

    private bool CaptureStoredSocialLinks(CallInfo updateCall)
    {
        _storedSocialLinks = updateCall.Arg<SocialLinks>();
        return true;
    }

    private void CreateProfileUseCases()
    {
        _profileUseCases = new ProfileUseCases(_profileRepository, _matchResultRepository, _avatarStorage);
    }

    private void FindProfileWithArrangedUsername()
    {
        _profileUseCases.FindProfileByUsername(_username);
    }

    private void GetStatisticsOfArrangedPlayer()
    {
        _profileUseCases.GetProfileStatistics(_playerId);
    }

    private void LoadAvatarImageOfArrangedProfile()
    {
        _profileUseCases.LoadProfileAvatarImage(_profile);
    }

    private void UpdateBiographyWithArrangedData()
    {
        _profileUseCases.UpdateProfileBiography(_playerId, _biography);
    }

    private void ChangeUsernameWithArrangedData()
    {
        _profileUseCases.ChangeProfileUsername(_playerId, _newUsername);
    }

    private void UpdateSocialLinksWithArrangedData()
    {
        _profileUseCases.UpdateProfileSocialLinks(_playerId, _socialLinks);
    }

    private void UpdateAvatarWithArrangedData()
    {
        _profileUseCases.UpdateProfileAvatar(_playerId, _imageBytes);
    }

    private void CatchExpectedException<TException>(Action operation)
        where TException : Exception
    {
        try
        {
            operation();
        }
        catch (TException exception)
        {
            _caughtException = exception;
        }
    }
}
