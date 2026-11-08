using System;
using System.Net.Mail;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class SignUpUseCases : ISignUpUseCases
{
    private const int MaximumEmailLength = 255;
    private const int MinimumPasswordLength = 8;

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
        string normalizedUsername = PlayerInputValidator.NormalizeUsername(username);
        string normalizedEmail = email?.Trim() ?? string.Empty;

        ValidateEmail(normalizedEmail);
        PlayerInputValidator.ValidatePassword(password, MinimumPasswordLength);

        bool isRegistered = RegisterPlayer(normalizedUsername, normalizedEmail, password);
        return isRegistered;
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
