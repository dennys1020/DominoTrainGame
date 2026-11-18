using System;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.State;
using DominoTrainGame.Server.Application.UseCases;

namespace DominoTrainGame.Server.Application.Tests.Unit.UseCases;

[TestFixture]
public sealed class SignUpUseCasesTests
{
    private const int ExpectedCallCount = 1;
    private const int MaximumUsernameLength = 45;
    private const int TooLongUsernameLength = MaximumUsernameLength + 1;
    private const int MaximumEmailLength = 255;
    private const int TooLongEmailLength = MaximumEmailLength + 1;
    private const int MinimumPasswordLength = 8;
    private const int ShortPasswordLength = MinimumPasswordLength - 1;
    private const int MaximumPasswordLength = 128;
    private const int TooLongPasswordLength = MaximumPasswordLength + 1;
    private const int CodeLifetimeMinutes = 10;
    private const int MaximumFailedAttempts = 5;
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
    private const string VerificationCode = "123456";
    private const string PaddedVerificationCode = " 123456 ";
    private const string WrongVerificationCode = "654321";
    private const string DatabaseErrorMessage = "The database is unavailable.";
    private const string EmailErrorMessage = "The email could not be sent.";

    private static readonly DateTime _currentTime;

    private IPlayerRepository _playerRepository;
    private IPasswordHasher _passwordHasher;
    private IVerificationCodeGenerator _codeGenerator;
    private IEmailSender _emailSender;
    private IClock _clock;
    private PendingSignUpRegistry _pendingSignUps;
    private SignUpUseCases _signUpUseCases;
    private string _username;
    private string _email;
    private string _password;

    static SignUpUseCasesTests()
    {
        _currentTime = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
    }

    [SetUp]
    public void SetUp()
    {
        _playerRepository = Substitute.For<IPlayerRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _codeGenerator = Substitute.For<IVerificationCodeGenerator>();
        _emailSender = Substitute.For<IEmailSender>();
        _clock = Substitute.For<IClock>();

        _playerRepository.IsRegistrationTaken(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
        _playerRepository.CreatePlayer(Arg.Any<PlayerRegistration>()).Returns(true);
        _passwordHasher.Hash(Arg.Any<string>()).Returns(StoredPasswordHash);
        _codeGenerator.Generate().Returns(VerificationCode);
        _clock.UtcNow.Returns(_currentTime);
        _pendingSignUps = new PendingSignUpRegistry(_clock);

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
    public void Test_Constructor_NullSignUpVerifier_ThrowsArgumentNullException()
    {
        Action createWithoutVerifier = () => new SignUpUseCases(_playerRepository, _passwordHasher, null);

        Assert.Throws<ArgumentNullException>(createWithoutVerifier);
    }

    [Test]
    public void Test_SignUp_NewPlayer_ReturnsTrue()
    {
        bool isPending = _signUpUseCases.SignUp(NewUsername, NewEmail, ValidPassword);

        Assert.That(isPending, Is.True);
    }

    [Test]
    public void Test_SignUp_NewPlayer_SendsVerificationCodeByEmail()
    {
        _signUpUseCases.SignUp(NewUsername, NewEmail, ValidPassword);

        _emailSender.Received(ExpectedCallCount).SendVerificationCode(NewEmail, VerificationCode);
    }

    [Test]
    public void Test_SignUp_NewPlayer_DoesNotCreatePlayerBeforeVerification()
    {
        _signUpUseCases.SignUp(NewUsername, NewEmail, ValidPassword);

        _playerRepository.DidNotReceive().CreatePlayer(Arg.Any<PlayerRegistration>());
    }

    [Test]
    public void Test_SignUp_UsernameOrEmailInUse_ReturnsFalse()
    {
        GivenRegistrationTaken();

        bool isPending = _signUpUseCases.SignUp(NewUsername, NewEmail, ValidPassword);

        Assert.That(isPending, Is.False);
    }

    [Test]
    public void Test_SignUp_UsernameOrEmailInUse_DoesNotSendEmail()
    {
        GivenRegistrationTaken();

        _signUpUseCases.SignUp(NewUsername, NewEmail, ValidPassword);

        _emailSender.DidNotReceive().SendVerificationCode(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public void Test_SignUp_NewPlayer_StoresPasswordHash()
    {
        _signUpUseCases.SignUp(NewUsername, NewEmail, ValidPassword);

        Assert.That(FindPendingRegistration().PasswordHash, Is.EqualTo(StoredPasswordHash));
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

        Assert.That(FindPendingRegistration().Username, Is.EqualTo(NewUsername));
    }

    [Test]
    public void Test_SignUp_PaddedEmail_StoresTrimmedEmail()
    {
        _signUpUseCases.SignUp(NewUsername, PaddedEmail, ValidPassword);

        Assert.That(FindPendingRegistration().Email, Is.EqualTo(NewEmail));
    }

    [Test]
    public void Test_SignUp_MaximumLengthUsername_ReturnsTrue()
    {
        bool isPending = _signUpUseCases.SignUp(CreateText(MaximumUsernameLength), NewEmail, ValidPassword);

        Assert.That(isPending, Is.True);
    }

    [Test]
    public void Test_SignUp_MaximumLengthEmail_ReturnsTrue()
    {
        string maximumLengthEmail = CreateEmail(MaximumEmailLength);

        bool isPending = _signUpUseCases.SignUp(NewUsername, maximumLengthEmail, ValidPassword);

        Assert.That(isPending, Is.True);
    }

    [Test]
    public void Test_SignUp_MinimumLengthPassword_ReturnsTrue()
    {
        bool isPending = _signUpUseCases.SignUp(NewUsername, NewEmail, CreateText(MinimumPasswordLength));

        Assert.That(isPending, Is.True);
    }

    [Test]
    public void Test_SignUp_MaximumLengthPassword_ReturnsTrue()
    {
        bool isPending = _signUpUseCases.SignUp(NewUsername, NewEmail, CreateText(MaximumPasswordLength));

        Assert.That(isPending, Is.True);
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
        _playerRepository.IsRegistrationTaken(Arg.Any<string>(), Arg.Any<string>())
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_SignUp_EmailUnavailable_ThrowsEmailUnavailableException()
    {
        _emailSender
            .When(sender => sender.SendVerificationCode(NewEmail, VerificationCode))
            .Throw(new EmailUnavailableException(EmailErrorMessage));

        Assert.Throws<EmailUnavailableException>(SignUpWithArrangedData);
    }

    [Test]
    public void Test_VerifyEmail_CorrectCode_ReturnsSuccess()
    {
        SignUpWithArrangedData();

        VerifyEmailStatus status = _signUpUseCases.VerifyEmail(NewEmail, VerificationCode);

        Assert.That(status, Is.EqualTo(VerifyEmailStatus.Success));
    }

    [Test]
    public void Test_VerifyEmail_CorrectCode_CreatesPendingPlayer()
    {
        SignUpWithArrangedData();
        PlayerRegistration pendingRegistration = FindPendingRegistration();

        _signUpUseCases.VerifyEmail(NewEmail, VerificationCode);

        _playerRepository.Received(ExpectedCallCount).CreatePlayer(pendingRegistration);
    }

    [Test]
    public void Test_VerifyEmail_PaddedEmailAndCode_CreatesPlayer()
    {
        SignUpWithArrangedData();

        VerifyEmailStatus status = _signUpUseCases.VerifyEmail(PaddedEmail, PaddedVerificationCode);

        Assert.That(status, Is.EqualTo(VerifyEmailStatus.Success));
    }

    [Test]
    public void Test_VerifyEmail_AfterSuccess_RemovesPendingSignUp()
    {
        SignUpWithArrangedData();
        _signUpUseCases.VerifyEmail(NewEmail, VerificationCode);

        VerifyEmailStatus status = _signUpUseCases.VerifyEmail(NewEmail, VerificationCode);

        Assert.That(status, Is.EqualTo(VerifyEmailStatus.RegistrationNotFound));
    }

    [Test]
    public void Test_VerifyEmail_WrongCode_ReturnsInvalidCode()
    {
        SignUpWithArrangedData();

        VerifyEmailStatus status = _signUpUseCases.VerifyEmail(NewEmail, WrongVerificationCode);

        Assert.That(status, Is.EqualTo(VerifyEmailStatus.InvalidCode));
    }

    [Test]
    public void Test_VerifyEmail_WrongCode_DoesNotCreatePlayer()
    {
        SignUpWithArrangedData();

        _signUpUseCases.VerifyEmail(NewEmail, WrongVerificationCode);

        _playerRepository.DidNotReceive().CreatePlayer(Arg.Any<PlayerRegistration>());
    }

    [Test]
    public void Test_VerifyEmail_ExpiredCode_ReturnsCodeExpired()
    {
        SignUpWithArrangedData();
        _clock.UtcNow.Returns(_currentTime.AddMinutes(CodeLifetimeMinutes));

        VerifyEmailStatus status = _signUpUseCases.VerifyEmail(NewEmail, VerificationCode);

        Assert.That(status, Is.EqualTo(VerifyEmailStatus.CodeExpired));
    }

    [Test]
    public void Test_VerifyEmail_TooManyWrongCodes_ReturnsTooManyAttempts()
    {
        SignUpWithArrangedData();

        for (int attempt = 0; attempt < MaximumFailedAttempts; attempt++)
        {
            _signUpUseCases.VerifyEmail(NewEmail, WrongVerificationCode);
        }

        VerifyEmailStatus status = _signUpUseCases.VerifyEmail(NewEmail, VerificationCode);

        Assert.That(status, Is.EqualTo(VerifyEmailStatus.TooManyAttempts));
    }

    [Test]
    public void Test_VerifyEmail_NoPendingSignUp_ReturnsRegistrationNotFound()
    {
        VerifyEmailStatus status = _signUpUseCases.VerifyEmail(NewEmail, VerificationCode);

        Assert.That(status, Is.EqualTo(VerifyEmailStatus.RegistrationNotFound));
    }

    [Test]
    public void Test_VerifyEmail_AccountTakenBeforeVerification_ReturnsAlreadyRegistered()
    {
        SignUpWithArrangedData();
        _playerRepository.CreatePlayer(Arg.Any<PlayerRegistration>()).Returns(false);

        VerifyEmailStatus status = _signUpUseCases.VerifyEmail(NewEmail, VerificationCode);

        Assert.That(status, Is.EqualTo(VerifyEmailStatus.AlreadyRegistered));
    }

    [Test]
    public void Test_VerifyEmail_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        SignUpWithArrangedData();
        _playerRepository.CreatePlayer(Arg.Any<PlayerRegistration>())
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Action verifyEmail = () => _signUpUseCases.VerifyEmail(NewEmail, VerificationCode);

        Assert.Throws<DatabaseUnavailableException>(verifyEmail);
    }

    [Test]
    public void Test_ResendVerificationCode_PendingSignUp_ReturnsTrue()
    {
        SignUpWithArrangedData();

        bool wasResent = _signUpUseCases.ResendVerificationCode(NewEmail);

        Assert.That(wasResent, Is.True);
    }

    [Test]
    public void Test_ResendVerificationCode_PendingSignUp_SendsNewCode()
    {
        SignUpWithArrangedData();
        _codeGenerator.Generate().Returns(WrongVerificationCode);

        _signUpUseCases.ResendVerificationCode(NewEmail);

        _emailSender.Received(ExpectedCallCount).SendVerificationCode(NewEmail, WrongVerificationCode);
    }

    [Test]
    public void Test_ResendVerificationCode_NewCode_ReplacesPreviousCode()
    {
        SignUpWithArrangedData();
        _codeGenerator.Generate().Returns(WrongVerificationCode);
        _signUpUseCases.ResendVerificationCode(NewEmail);

        VerifyEmailStatus status = _signUpUseCases.VerifyEmail(NewEmail, VerificationCode);

        Assert.That(status, Is.EqualTo(VerifyEmailStatus.InvalidCode));
    }

    [Test]
    public void Test_ResendVerificationCode_AfterTooManyAttempts_AllowsVerification()
    {
        SignUpWithArrangedData();

        for (int attempt = 0; attempt < MaximumFailedAttempts; attempt++)
        {
            _signUpUseCases.VerifyEmail(NewEmail, WrongVerificationCode);
        }

        _signUpUseCases.ResendVerificationCode(NewEmail);

        VerifyEmailStatus status = _signUpUseCases.VerifyEmail(NewEmail, VerificationCode);

        Assert.That(status, Is.EqualTo(VerifyEmailStatus.Success));
    }

    [Test]
    public void Test_ResendVerificationCode_NoPendingSignUp_ReturnsFalse()
    {
        bool wasResent = _signUpUseCases.ResendVerificationCode(NewEmail);

        Assert.That(wasResent, Is.False);
    }

    [Test]
    public void Test_ResendVerificationCode_EmailUnavailable_ThrowsEmailUnavailableException()
    {
        SignUpWithArrangedData();
        _codeGenerator.Generate().Returns(WrongVerificationCode);
        _emailSender
            .When(sender => sender.SendVerificationCode(NewEmail, WrongVerificationCode))
            .Throw(new EmailUnavailableException(EmailErrorMessage));

        Action resendCode = () => _signUpUseCases.ResendVerificationCode(NewEmail);

        Assert.Throws<EmailUnavailableException>(resendCode);
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
        SignUpVerifier signUpVerifier = new SignUpVerifier(_pendingSignUps, _codeGenerator, _emailSender);
        _signUpUseCases = new SignUpUseCases(_playerRepository, _passwordHasher, signUpVerifier);
    }

    private void SignUpWithArrangedData()
    {
        _signUpUseCases.SignUp(_username, _email, _password);
    }

    private void GivenRegistrationTaken()
    {
        _playerRepository.IsRegistrationTaken(NewUsername, NewEmail).Returns(true);
    }

    private PlayerRegistration FindPendingRegistration()
    {
        _pendingSignUps.TryFind(NewEmail, out PendingSignUp pendingSignUp);
        PlayerRegistration registration = pendingSignUp.Registration;
        return registration;
    }
}
