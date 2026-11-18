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

        bool isAvailable = IsAccountAvailable(normalizedUsername, normalizedEmail);

        if (isAvailable)
        {
            string passwordHash = _passwordHasher.Hash(password);
            _signUpVerifier.SendCode(new PlayerRegistration(normalizedUsername, normalizedEmail, passwordHash));
        }

        LogSignUpAttempt(isAvailable);
        return isAvailable;
    }

    public VerifyEmailStatus VerifyEmail(string email, string verificationCode)
    {
        string normalizedEmail = NormalizeEmail(email);
        string normalizedCode = verificationCode?.Trim() ?? string.Empty;
        VerifyEmailStatus status = _signUpVerifier.VerifyAccount(normalizedEmail, normalizedCode);
        bool isCodeAccepted = status == VerifyEmailStatus.Success;

        if (isCodeAccepted)
        {
            status = CompleteRegistration(normalizedEmail);
        }

        _logger.InfoFormat("An email verification finished. Status: {0}.", status);
        return status;
    }

    public bool ResendVerificationCode(string email)
    {
        string normalizedEmail = NormalizeEmail(email);
        bool wasResent = _signUpVerifier.ResendCode(normalizedEmail);

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

    private static void LogSignUpAttempt(bool isAvailable)
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

    private bool IsAccountAvailable(string username, string email)
    {
        bool isAvailable = !_playerRepository.IsRegistrationTaken(username, email);
        return isAvailable;
    }

    private VerifyEmailStatus CompleteRegistration(string email)
    {
        bool isPending = _signUpVerifier.TryFindPendingSignUp(email, out PendingSignUp pendingSignUp);
        bool isRegistered = false;

        if (isPending)
        {
            isRegistered = _playerRepository.CreatePlayer(pendingSignUp.Registration);
        }

        _signUpVerifier.DiscardPendingSignUp(email);

        VerifyEmailStatus status = isRegistered ? VerifyEmailStatus.Success : VerifyEmailStatus.AlreadyRegistered;
        return status;
    }
}
