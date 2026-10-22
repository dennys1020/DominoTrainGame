using System;
using System.Net.Mail;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.Authentication;

public sealed class SignUpUseCases : ISignUpUseCases
{
    private const int MaximumUsernameLength = 45;
    private const int MaximumEmailLength = 255;
    private const int MinimumPasswordLength = 8;
    private const int MaximumPasswordLength = 128;

    private static readonly ITraceLog _logger;

    private readonly IPlayerRepository _playerRepository;
    private readonly IPasswordHasher _passwordHasher;

    static SignUpUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(SignUpUseCases));
    }

    public SignUpUseCases(IPlayerRepository playerRepository, IPasswordHasher passwordHasher)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
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

    private bool RegisterPlayer(string username, string email, string password)
    {
        string passwordHash = _passwordHasher.Hash(password);
        PlayerRegistration registration = new PlayerRegistration(username, email, passwordHash);
        bool isRegistered = _playerRepository.CreatePlayer(registration);

        LogSignUpAttempt(isRegistered);
        return isRegistered;
    }
}
