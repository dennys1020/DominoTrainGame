using System;
using System.Net.Mail;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Errors;

namespace DominoTrainGame.Server.Application.Authentication;

public sealed class SignUpCases : ISignUpUseCases
{
    private const int MaximumUsernameLength = 45;
    private const int MaximumEmailLength = 255;
    private const int MinimumPasswordLength = 8;
    private const int MaximumPasswordLength = 128;
    private static readonly ITraceLog _logger;
    private readonly IPlayerRepository _playerRepository;

    static SignUpCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(SignUpCases));
    }

    public SignUpCases(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public bool SignUp(string username, string email, string password)
    {
        string normalizedUsername = username?.Trim() ?? string.Empty;
        string normalizedEmail = email?.Trim() ?? string.Empty;

        ValidateUsername(normalizedUsername);
        ValidateEmail(normalizedEmail);
        ValidatePassword(password);

        bool isRegistered = RegisterPlayer(normalizedUsername, normalizedEmail, password);
        return isRegistered;
    }

    private static void ValidateUsername(string username)
    {
        bool isUsernameValid = !string.IsNullOrWhiteSpace(username)
            && username.Length <= MaximumUsernameLength;

        if (!isUsernameValid)
        {
            throw new ArgumentException(
                "The username is empty or exceeds the maximum allowed length.",
                nameof(username));
        }
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

    private static void ValidatePassword(string password)
    {
        bool isPasswordValid = !string.IsNullOrEmpty(password)
            && password.Length >= MinimumPasswordLength
            && password.Length <= MaximumPasswordLength;

        if (!isPasswordValid)
        {
            throw new ArgumentException(
                "The password is shorter or longer than the allowed length.",
                nameof(password));
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

    private bool RegisterPlayer(string username, string email, string password)
    {
        bool isRegistered;

        try
        {
            bool isAvailable = !_playerRepository.IsUsernameTaken(username)
                && !_playerRepository.IsEmailTaken(email);

            isRegistered = isAvailable;

            if (isAvailable)
            {
                string passwordHash = PasswordHasher.Hash(password);
                _playerRepository.AddPlayer(username, email, passwordHash);
            }

            LogSignUpAttempt(isRegistered);
        }
        catch (DatabaseOperationException exception)
        {
            throw new DatabaseUnavailableException(
                "The player could not be registered in the database.",
                exception);
        }

        return isRegistered;
    }

    private static void LogSignUpAttempt(bool isRegistered)
    {
        if (isRegistered)
        {
            _logger.Info("A new player was registered.");
        }
        else
        {
            _logger.Info("A sign up request was rejected because the username or email is already in use.");
        }
    }
}
