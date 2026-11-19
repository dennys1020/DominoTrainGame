using System;
using System.Net.Mail;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.State;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class SignUpUseCases : ISignUpUseCases
{
    private const int MaximumEmailLength = 255;
    private const int MinimumPasswordLength = 8;

    private static readonly ITraceLog _logger;

    private readonly IPlayerRepository _playerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly PendingSignUpRegistry _pendingSignUps;
    private readonly IVerificationCodeGenerator _codeGenerator;
    private readonly IEmailSender _emailSender;

    static SignUpUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(SignUpUseCases));
    }

    public SignUpUseCases(
        IPlayerRepository playerRepository,
        IPasswordHasher passwordHasher,
        PendingSignUpRegistry pendingSignUps,
        IVerificationCodeGenerator codeGenerator,
        IEmailSender emailSender)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _pendingSignUps = pendingSignUps ?? throw new ArgumentNullException(nameof(pendingSignUps));
        _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    public bool SignUp(SignUpDetails detailsS, string language)
    {
        if (details == null)
        {
            throw new ArgumentNullException(nameof(details));
        }

        string normalizedUsername = PlayerInputValidator.NormalizeUsername(details.Username);
        string normalizedEmail = NormalizeEmail(details.Email);

        ValidateEmail(normalizedEmail);
        PlayerInputValidator.ValidatePassword(details.Password, MinimumPasswordLength);

        bool isAvailable = IsUsernameAndEmailAvailable(normalizedUsername, normalizedEmail);

        if (isAvailable)
        {
            string passwordHash = _passwordHasher.Hash(details.Password);
            PlayerRegistration registration = new PlayerRegistration(normalizedUsername, normalizedEmail, passwordHash);

            SendVerificationCode(registration, language);
        }

        LogVerificationCodeRequest(isAvailable);
        return isAvailable;
    }

    public VerifyEmailStatus VerifyEmail(string email, string verificationCode)
    {
        string normalizedEmail = NormalizeEmail(email);
        string normalizedCode = verificationCode?.Trim() ?? string.Empty;
        VerifyEmailStatus emailStatus = _pendingSignUps.VerifyPendingSignUpCode(normalizedEmail, normalizedCode);
        bool isCodeAccepted = emailStatus == VerifyEmailStatus.Success;

        if (isCodeAccepted)
        {
            emailStatus = RegisterVerifiedPlayer(normalizedEmail);
        }

        _logger.InfoFormat("An email verification finished. Status: {0}.", emailStatus);
        return emailStatus;
    }

    public ResendVerificationCodeStatus ResendVerificationCode(string email, string language)
    {
        string normalizedEmail = NormalizeEmail(email);
        PendingSignUp pendingSignUp = _pendingSignUps.FindPendingSignUpByEmail(normalizedEmail);
        ResendVerificationCodeStatus resendStatus = GetResendStatus(pendingSignUp);
        bool canResend = resendStatus == ResendVerificationCodeStatus.Success;

        if (canResend)
        {
            SendVerificationCode(pendingSignUp.Registration, language);
        }

        _logger.InfoFormat("A verification code resend finished. Status: {0}.", resendStatus);
        return resendStatus;
    }

    private static string NormalizeEmail(string email)
    {
        string normalizedEmail = email?.Trim() ?? string.Empty;
        return normalizedEmail;
    }

    private static void ValidateEmail(string email)
    {
        bool isEmailValid = !string.IsNullOrWhiteSpace(email)
            && email.Length <= MaximumEmailLength
            && IsWellFormedEmail(email);

        if (!isEmailValid)
        {
            throw new ArgumentException(
                "The email is empty, malformed or exceeds the maximum allowed length.",
                nameof(email));
        }
    }

    private static bool IsWellFormedEmail(string email)
    {
        bool isWellFormed;

        try
        {
            MailAddress address = new MailAddress(email);
            isWellFormed = string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            isWellFormed = false;
        }

        return isWellFormed;
    }

    private static void LogVerificationCodeRequest(bool isAvailable)
    {
        if (isAvailable)
        {
            _logger.Info("A verification code was sent for a new sign up.");
        }
        else
        {
            _logger.Info("A sign up request was rejected because the username or email is already in use.");
        }
    }

    private void SendVerificationCode(PlayerRegistration registration, string language)
    {
        string verificationCode = _codeGenerator.Generate();

        _emailSender.SendVerificationCode(registration.Email, verificationCode, language);
        _pendingSignUps.StorePendingSignUp(registration, verificationCode);
    }

    private ResendVerificationCodeStatus GetResendStatus(PendingSignUp pendingSignUp)
    {
        ResendVerificationCodeStatus resendStatus = ResendVerificationCodeStatus.RegistrationNotFound;

        if (!pendingSignUp.IsEmpty)
        {
            bool isAvailable = _pendingSignUps.IsResendAvailable(pendingSignUp);
            resendStatus = isAvailable ? ResendVerificationCodeStatus.Success : ResendVerificationCodeStatus.TooSoon;
        }

        return resendStatus;
    }

    private bool IsUsernameAndEmailAvailable(string username, string email)
    {
        bool isAvailable = !_playerRepository.IsRegistrationTaken(username, email);
        return isAvailable;
    }

    private VerifyEmailStatus RegisterVerifiedPlayer(string email)
    {
        PendingSignUp pendingSignUp = _pendingSignUps.FindPendingSignUpByEmail(email);
        bool isRegistered = false;

        if (!pendingSignUp.IsEmpty)
        {
            isRegistered = _playerRepository.CreatePlayer(pendingSignUp.Registration);
        }

        _pendingSignUps.RemovePendingSignUp(email);

        VerifyEmailStatus status = isRegistered ? VerifyEmailStatus.Success : VerifyEmailStatus.AlreadyRegistered;
        return status;
    }
}
