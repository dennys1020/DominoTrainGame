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
    private readonly SignUpVerifier _signUpVerifier;

    static SignUpUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(SignUpUseCases));
    }

    public SignUpUseCases(
        IPlayerRepository playerRepository,
        IPasswordHasher passwordHasher,
        SignUpVerifier signUpVerifier)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _signUpVerifier = signUpVerifier ?? throw new ArgumentNullException(nameof(signUpVerifier));
    }

    public bool SignUp(string username, string email, string password)
    {
        string normalizedUsername = PlayerInputValidator.NormalizeUsername(username);
        string normalizedEmail = NormalizeEmail(email);

        ValidateEmail(normalizedEmail);
        PlayerInputValidator.ValidatePassword(password, MinimumPasswordLength);

        bool isAvailable = IsUsernameAndEmailAvailable(normalizedUsername, normalizedEmail);

        if (isAvailable)
        {
            string passwordHash = _passwordHasher.Hash(password);
            PlayerRegistration registration = new PlayerRegistration(normalizedUsername, normalizedEmail, passwordHash);

            _signUpVerifier.SendVerificationCode(registration);
        }

        LogVerificationCodeRequest(isAvailable);
        return isAvailable;
    }

    public VerifyEmailStatus VerifyEmail(string email, string verificationCode)
    {
        string normalizedEmail = NormalizeEmail(email);
        string normalizedCode = verificationCode?.Trim() ?? string.Empty;
        VerifyEmailStatus emailStatus = _signUpVerifier.VerifyPendingSignUpCode(normalizedEmail, normalizedCode);
        bool isCodeAccepted = emailStatus == VerifyEmailStatus.Success;

        if (isCodeAccepted)
        {
            emailStatus = RegisterVerifiedPlayer(normalizedEmail);
        }

        _logger.InfoFormat("An email verification finished. Status: {0}.", emailStatus);
        return emailStatus;
    }

    public bool ResendVerificationCode(string email)
    {
        string normalizedEmail = NormalizeEmail(email);
        bool wasResent = _signUpVerifier.ResendVerificationCode(normalizedEmail);

        _logger.InfoFormat("A verification code resend finished. Resent: {0}.", wasResent);
        return wasResent;
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

    private bool IsUsernameAndEmailAvailable(string username, string email)
    {
        bool isAvailable = !_playerRepository.IsRegistrationTaken(username, email);
        return isAvailable;
    }

    private VerifyEmailStatus RegisterVerifiedPlayer(string email)
    {
        PendingSignUp pendingSignUp = _signUpVerifier.FindPendingSignUpByEmail(email);
        bool isRegistered = false;

        if (!pendingSignUp.IsEmpty)
        {
            isRegistered = _playerRepository.CreatePlayer(pendingSignUp.Registration);
        }

        _signUpVerifier.DiscardPendingSignUp(email);

        VerifyEmailStatus status = isRegistered ? VerifyEmailStatus.Success : VerifyEmailStatus.AlreadyRegistered;
        return status;
    }
}
