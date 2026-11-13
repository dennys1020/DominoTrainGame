using System;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class LoginUseCases : ILoginUseCases
{
    private const int MaximumIdentifierLength = 255;
    private const int MinimumPasswordLength = 1;

    private static readonly ITraceLog _logger;

    private readonly IPlayerRepository _playerRepository;
    private readonly IPasswordHasher _passwordHasher;

    static LoginUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(LoginUseCases));
    }

    public LoginUseCases(IPlayerRepository playerRepository, IPasswordHasher passwordHasher)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public PlayerIdentity LogIn(string identifier, string password)
    {
        string normalizedIdentifier = identifier?.Trim() ?? string.Empty;

        ValidateIdentifier(normalizedIdentifier);
        PlayerInputValidator.ValidatePassword(password, MinimumPasswordLength);

        PlayerIdentity player = VerifyCredentials(normalizedIdentifier, password);
        return player;
    }

    private static void ValidateIdentifier(string identifier)
    {
        bool isIdentifierValid = !string.IsNullOrWhiteSpace(identifier)
            && identifier.Length <= MaximumIdentifierLength;

        if (!isIdentifierValid)
        {
            throw new ArgumentException(
                "The identifier is empty or exceeds the maximum allowed length.",
                nameof(identifier));
        }
    }

    private PlayerIdentity VerifyCredentials(string identifier, string password)
    {
        PlayerCredentials credentials = _playerRepository.FindCredentials(identifier);
        bool isAuthenticated = !credentials.IsEmpty && _passwordHasher.Verify(credentials.PasswordHash, password);

        LogLoginAttempt(isAuthenticated, credentials);

        PlayerIdentity player = isAuthenticated
            ? new PlayerIdentity(credentials.PlayerId, credentials.Username)
            : PlayerIdentity.Empty;
        return player;
    }

    private static void LogLoginAttempt(bool isAuthenticated, PlayerCredentials credentials)
    {
        if (isAuthenticated)
        {
            _logger.InfoFormat("A player logged in. PlayerId: {0}.", credentials.PlayerId);
        }
        else
        {
            _logger.Info("A login attempt was rejected because the credentials are invalid.");
        }
    }
}
