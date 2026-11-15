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
public sealed class LoginUseCasesTests
{
    private const int ExistingPlayerId = 7;
    private const int ExpectedCallCount = 1;
    private const int MaximumIdentifierLength = 255;
    private const int TooLongIdentifierLength = MaximumIdentifierLength + 1;
    private const int MaximumPasswordLength = 128;
    private const int TooLongPasswordLength = MaximumPasswordLength + 1;
    private const char FillerCharacter = 'a';
    private const string ExistingUsername = "samuel";
    private const string ExistingEmail = "samuel@uv.mx";
    private const string PaddedUsername = "  samuel  ";
    private const string UnknownUsername = "nobody";
    private const string EmptyText = "";
    private const string WhitespaceIdentifier = "   ";
    private const string PaddingText = " ";
    private const string StoredPasswordHash = "stored-hash";
    private const string ValidPassword = "Secret123";
    private const string WrongPassword = "Wrong123";
    private const string PaddedPassword = "  Secret123  ";
    private const string SingleCharacterPassword = "x";
    private const string DatabaseErrorMessage = "The database is unavailable.";

    private IPlayerRepository _playerRepository;
    private IPasswordHasher _passwordHasher;
    private LoginUseCases _loginUseCases;
    private string _identifier;
    private string _password;

    [SetUp]
    public void SetUp()
    {
        _playerRepository = Substitute.For<IPlayerRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        PlayerCredentials credentials = new PlayerCredentials(ExistingPlayerId, ExistingUsername, StoredPasswordHash);

        _playerRepository.FindCredentials(Arg.Any<string>()).Returns(PlayerCredentials.Empty);
        _playerRepository.FindCredentials(ExistingUsername).Returns(credentials);
        _playerRepository.FindCredentials(ExistingEmail).Returns(credentials);
        _passwordHasher.Verify(StoredPasswordHash, ValidPassword).Returns(true);

        _identifier = ExistingUsername;
        _password = ValidPassword;
        CreateLoginUseCases();
    }

    [Test]
    public void Test_Constructor_NullPlayerRepository_ThrowsArgumentNullException()
    {
        _playerRepository = null;

        Assert.Throws<ArgumentNullException>(CreateLoginUseCases);
    }

    [Test]
    public void Test_Constructor_NullPasswordHasher_ThrowsArgumentNullException()
    {
        _passwordHasher = null;

        Assert.Throws<ArgumentNullException>(CreateLoginUseCases);
    }

    [Test]
    public void Test_LogIn_ValidUsernameAndPassword_ReturnsPlayerId()
    {
        PlayerIdentity player = _loginUseCases.LogIn(ExistingUsername, ValidPassword);

        Assert.That(player.PlayerId, Is.EqualTo(ExistingPlayerId));
    }

    [Test]
    public void Test_LogIn_ValidUsernameAndPassword_ReturnsUsername()
    {
        PlayerIdentity player = _loginUseCases.LogIn(ExistingUsername, ValidPassword);

        Assert.That(player.Username, Is.EqualTo(ExistingUsername));
    }

    [Test]
    public void Test_LogIn_ValidEmailAndPassword_ReturnsStoredUsername()
    {
        PlayerIdentity player = _loginUseCases.LogIn(ExistingEmail, ValidPassword);

        Assert.That(player.Username, Is.EqualTo(ExistingUsername));
    }

    [Test]
    public void Test_LogIn_WrongPassword_ReturnsEmptyIdentity()
    {
        PlayerIdentity player = _loginUseCases.LogIn(ExistingUsername, WrongPassword);

        Assert.That(player.IsEmpty, Is.True);
    }

    [Test]
    public void Test_LogIn_UnknownUser_ReturnsEmptyIdentity()
    {
        PlayerIdentity player = _loginUseCases.LogIn(UnknownUsername, ValidPassword);

        Assert.That(player.IsEmpty, Is.True);
    }

    [Test]
    public void Test_LogIn_UnknownUser_DoesNotVerifyPassword()
    {
        _loginUseCases.LogIn(UnknownUsername, ValidPassword);

        _passwordHasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public void Test_LogIn_PaddedIdentifier_SearchesTrimmedIdentifier()
    {
        _loginUseCases.LogIn(PaddedUsername, ValidPassword);

        _playerRepository.Received(ExpectedCallCount).FindCredentials(ExistingUsername);
    }

    [Test]
    public void Test_LogIn_PaddedPassword_VerifiesPasswordWithoutTrimming()
    {
        _loginUseCases.LogIn(ExistingUsername, PaddedPassword);

        _passwordHasher.Received(ExpectedCallCount).Verify(StoredPasswordHash, PaddedPassword);
    }

    [Test]
    public void Test_LogIn_MaximumLengthIdentifier_SearchesCredentials()
    {
        string maximumLengthIdentifier = CreateText(MaximumIdentifierLength);

        _loginUseCases.LogIn(maximumLengthIdentifier, ValidPassword);

        _playerRepository.Received(ExpectedCallCount).FindCredentials(maximumLengthIdentifier);
    }

    [Test]
    public void Test_LogIn_PaddedMaximumLengthIdentifier_SearchesTrimmedIdentifier()
    {
        string maximumLengthIdentifier = CreateText(MaximumIdentifierLength);
        string paddedIdentifier = PaddingText + maximumLengthIdentifier + PaddingText;

        _loginUseCases.LogIn(paddedIdentifier, ValidPassword);

        _playerRepository.Received(ExpectedCallCount).FindCredentials(maximumLengthIdentifier);
    }

    [Test]
    public void Test_LogIn_SingleCharacterPassword_VerifiesPassword()
    {
        _loginUseCases.LogIn(ExistingUsername, SingleCharacterPassword);

        _passwordHasher.Received(ExpectedCallCount).Verify(StoredPasswordHash, SingleCharacterPassword);
    }

    [Test]
    public void Test_LogIn_MaximumLengthPassword_VerifiesPassword()
    {
        string maximumLengthPassword = CreateText(MaximumPasswordLength);

        _loginUseCases.LogIn(ExistingUsername, maximumLengthPassword);

        _passwordHasher.Received(ExpectedCallCount).Verify(StoredPasswordHash, maximumLengthPassword);
    }

    [Test]
    public void Test_LogIn_NullIdentifier_ThrowsArgumentException()
    {
        _identifier = null;

        Assert.Throws<ArgumentException>(LogInWithArrangedCredentials);
    }

    [Test]
    public void Test_LogIn_EmptyIdentifier_ThrowsArgumentException()
    {
        _identifier = EmptyText;

        Assert.Throws<ArgumentException>(LogInWithArrangedCredentials);
    }

    [Test]
    public void Test_LogIn_WhitespaceIdentifier_ThrowsArgumentException()
    {
        _identifier = WhitespaceIdentifier;

        Assert.Throws<ArgumentException>(LogInWithArrangedCredentials);
    }

    [Test]
    public void Test_LogIn_TooLongIdentifier_ThrowsArgumentException()
    {
        _identifier = CreateText(TooLongIdentifierLength);

        Assert.Throws<ArgumentException>(LogInWithArrangedCredentials);
    }

    [Test]
    public void Test_LogIn_NullPassword_ThrowsArgumentException()
    {
        _password = null;

        Assert.Throws<ArgumentException>(LogInWithArrangedCredentials);
    }

    [Test]
    public void Test_LogIn_EmptyPassword_ThrowsArgumentException()
    {
        _password = EmptyText;

        Assert.Throws<ArgumentException>(LogInWithArrangedCredentials);
    }

    [Test]
    public void Test_LogIn_TooLongPassword_ThrowsArgumentException()
    {
        _password = CreateText(TooLongPasswordLength);

        Assert.Throws<ArgumentException>(LogInWithArrangedCredentials);
    }

    [Test]
    public void Test_LogIn_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _playerRepository.FindCredentials(ExistingUsername)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(LogInWithArrangedCredentials);
    }

    private static string CreateText(int length)
    {
        string text = new string(FillerCharacter, length);
        return text;
    }

    private void CreateLoginUseCases()
    {
        _loginUseCases = new LoginUseCases(_playerRepository, _passwordHasher);
    }

    private void LogInWithArrangedCredentials()
    {
        _loginUseCases.LogIn(_identifier, _password);
    }
}
