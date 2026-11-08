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

    public bool LogIn(string identifier, string password, out PlayerIdentity player)
    {
        string normalizedIdentifier = identifier?.Trim() ?? string.Empty;

        ValidateIdentifier(normalizedIdentifier);
        PlayerInputValidator.ValidatePassword(password, MinimumPasswordLength);

        bool isAuthenticated = VerifyCredentials(normalizedIdentifier, password, out player);
        return isAuthenticated;
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

    private bool VerifyCredentials(string identifier, string password, out PlayerIdentity player)
    {
        bool isRegistered = _playerRepository.TryFindCredentials(identifier, out PlayerCredentials credentials);
        bool isAuthenticated = isRegistered && _passwordHasher.Verify(credentials.PasswordHash, password);

        LogLoginAttempt(isAuthenticated, credentials);

        player = isAuthenticated
            ? new PlayerIdentity(credentials.PlayerId, credentials.Username)
            : PlayerIdentity.Empty;
        return isAuthenticated;
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
