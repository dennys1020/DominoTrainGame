using System;
using System.Linq;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.UseCases;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace DominoTrainGame.Server.Application.Tests.Unit.UseCases;

[TestFixture]
public sealed class SignUpUseCasesTests
{
    private const int ExpectedCallCount = 1;
    private const int RegistrationArgumentIndex = 0;
    private const int MaximumUsernameLength = 45;
    private const int TooLongUsernameLength = MaximumUsernameLength + 1;
    private const int MaximumEmailLength = 255;
    private const int TooLongEmailLength = MaximumEmailLength + 1;
    private const int MinimumPasswordLength = 8;
    private const int ShortPasswordLength = MinimumPasswordLength - 1;
    private const int MaximumPasswordLength = 128;
    private const int TooLongPasswordLength = MaximumPasswordLength + 1;
    private const char FillerCharacter = 'a';
    private const string NewUsername = "samuel";
    private const string PaddedUsername = "  samuel  ";
    private const string WhitespaceText = "   ";
    private const string EmailDomain = "@uv.mx";
    private const string NewEmail = "samuel@uv.mx";
    private const string PaddedEmail = "  samuel@uv.mx  ";
    private const string EmailWithoutAtSign = "samuel.uv.mx";
    private const string EmailWithDisplayName = "Samuel <samuel@uv.mx>";
    private const string ValidPassword = "Secret123";
    private const string PaddedPassword = "  Secret123  ";
    private const string StoredPasswordHash = "stored-hash";
    private const string DatabaseErrorMessage = "The database is unavailable.";

    private IPlayerRepository _playerRepository;
    private IPasswordHasher _passwordHasher;
    private SignUpUseCases _signUpUseCases;
    private string _username;
    private string _email;
    private string _password;

    [SetUp]
    public void SetUp()
    {
        _playerRepository = Substitute.For<IPlayerRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();

        _playerRepository.CreatePlayer(Arg.Any<PlayerRegistration>()).Returns(true);
        _passwordHasher.Hash(Arg.Any<string>()).Returns(StoredPasswordHash);

        _username = NewUsername;
        _email = NewEmail;
        _password = ValidPassword;
        CreateSignUpUseCases();
    }

    [Test]
    public void Test_Constructor_NullPlayerRepository_ThrowsArgumentNullException()
    {
        _playerRepository = null;

        Assert.Throws<ArgumentNullException>(CreateSignUpUseCases);
    }

    [Test]
    public void Test_Constructor_NullPasswordHasher_ThrowsArgumentNullException()
    {
        _passwordHasher = null;

        Assert.Throws<ArgumentNullException>(CreateSignUpUseCases);
    }

    [Test]
    public void Test_SignUp_NewPlayer_ReturnsTrue()
    {
        bool isRegistered = _signUpUseCases.SignUp(NewUsername, NewEmail, ValidPassword);

        Assert.That(isRegistered, Is.True);
    }

    [Test]
    public void Test_SignUp_UsernameOrEmailInUse_ReturnsFalse()
    {
        _playerRepository.CreatePlayer(Arg.Any<PlayerRegistration>()).Returns(false);

        bool isRegistered = _signUpUseCases.SignUp(NewUsername, NewEmail, ValidPassword);

        Assert.That(isRegistered, Is.False);
    }

    [Test]
    public void Test_SignUp_NewPlayer_StoresPasswordHash()
    {
        _signUpUseCases.SignUp(NewUsername, NewEmail, ValidPassword);

        Assert.That(FindStoredRegistration().PasswordHash, Is.EqualTo(StoredPasswordHash));
    }

    [Test]
    public void Test_SignUp_PaddedPassword_HashesPasswordWithoutTrimming()
    {
        _signUpUseCases.SignUp(NewUsername, NewEmail, PaddedPassword);

        _passwordHasher.Received(ExpectedCallCount).Hash(PaddedPassword);
    }

    [Test]
    public void Test_SignUp_PaddedUsername_StoresTrimmedUsername()
    {
        _signUpUseCases.SignUp(PaddedUsername, NewEmail, ValidPassword);

        Assert.That(FindStoredRegistration().Username, Is.EqualTo(NewUsername));
    }

    [Test]
    public void Test_SignUp_PaddedEmail_StoresTrimmedEmail()
    {
        _signUpUseCases.SignUp(NewUsername, PaddedEmail, ValidPassword);

        Assert.That(FindStoredRegistration().Email, Is.EqualTo(NewEmail));
    }

    [Test]
    public void Test_SignUp_MaximumLengthUsername_ReturnsTrue()
    {
        bool isRegistered = _signUpUseCases.SignUp(CreateText(MaximumUsernameLength), NewEmail, ValidPassword);

        Assert.That(isRegistered, Is.True);
    }

    [Test]
    public void Test_SignUp_MaximumLengthEmail_ReturnsTrue()
    {
        string maximumLengthEmail = CreateEmail(MaximumEmailLength);

        bool isRegistered = _signUpUseCases.SignUp(NewUsername, maximumLengthEmail, ValidPassword);

        Assert.That(isRegistered, Is.True);
    }

    [Test]
    public void Test_SignUp_MinimumLengthPassword_ReturnsTrue()
    {
        bool isRegistered = _signUpUseCases.SignUp(NewUsername, NewEmail, CreateText(MinimumPasswordLength));

        Assert.That(isRegistered, Is.True);
    }

    [Test]
    public void Test_SignUp_MaximumLengthPassword_ReturnsTrue()
    {
        bool isRegistered = _signUpUseCases.SignUp(NewUsername, NewEmail, CreateText(MaximumPasswordLength));

        Assert.That(isRegistered, Is.True);
    }

    [Test]
    public void Test_SignUp_NullUsername_ThrowsArgumentException()
    {
        _username = null;

        Assert.Throws<ArgumentException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_WhitespaceUsername_ThrowsArgumentException()
    {
        _username = WhitespaceText;

        Assert.Throws<ArgumentException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_TooLongUsername_ThrowsArgumentException()
    {
        _username = CreateText(TooLongUsernameLength);

        Assert.Throws<ArgumentException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_NullEmail_ThrowsArgumentException()
    {
        _email = null;

        Assert.Throws<ArgumentException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_WhitespaceEmail_ThrowsArgumentException()
    {
        _email = WhitespaceText;

        Assert.Throws<ArgumentException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_EmailWithoutAtSign_ThrowsArgumentException()
    {
        _email = EmailWithoutAtSign;

        Assert.Throws<ArgumentException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_EmailWithDisplayName_ThrowsArgumentException()
    {
        _email = EmailWithDisplayName;

        Assert.Throws<ArgumentException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_TooLongEmail_ThrowsArgumentException()
    {
        _email = CreateEmail(TooLongEmailLength);

        Assert.Throws<ArgumentException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_NullPassword_ThrowsArgumentException()
    {
        _password = null;

        Assert.Throws<ArgumentException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_ShortPassword_ThrowsArgumentException()
    {
        _password = CreateText(ShortPasswordLength);

        Assert.Throws<ArgumentException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_TooLongPassword_ThrowsArgumentException()
    {
        _password = CreateText(TooLongPasswordLength);

        Assert.Throws<ArgumentException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        _playerRepository.CreatePlayer(Arg.Any<PlayerRegistration>())
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(SignUpWithArrangedData);
    }

    private static string CreateText(int length)
    {
        string text = new string(FillerCharacter, length);
        return text;
    }

    private static string CreateEmail(int length)
    {
        string email = CreateText(length - EmailDomain.Length) + EmailDomain;
        return email;
    }

    private void CreateSignUpUseCases()
    {
        _signUpUseCases = new SignUpUseCases(_playerRepository, _passwordHasher);
    }

    private void SignUpWithArrangedData()
    {
        _signUpUseCases.SignUp(_username, _email, _password);
    }

    private PlayerRegistration FindStoredRegistration()
    {
        ICall createPlayerCall = _playerRepository.ReceivedCalls().Single();
        PlayerRegistration registration =
            (PlayerRegistration)createPlayerCall.GetArguments()[RegistrationArgumentIndex];
        return registration;
    }
}
