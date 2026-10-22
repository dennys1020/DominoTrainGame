using System;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Persistence.Errors;
using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Application.Authentication;

public sealed class LoginUseCases : ILoginUseCases
{
    private const int MaximumIdentifierLength = 255;
    private const int MaximumPasswordLength = 128;
    private static readonly ITraceLog _logger;
    private readonly IPlayerRepository _playerRepository;

    static LoginUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(LoginUseCases));
    }

    public LoginUseCases(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public bool LogIn(string identifier, string password)
    {
        string normalizedIdentifier = identifier?.Trim() ?? string.Empty;

        ValidateIdentifier(normalizedIdentifier);
        ValidatePassword(password);

        bool isAuthenticated = VerifyCredentials(normalizedIdentifier, password);
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

    private static void ValidatePassword(string password)
    {
        bool isPasswordValid = !string.IsNullOrEmpty(password)
            && password.Length <= MaximumPasswordLength;

        if (!isPasswordValid)
        {
            throw new ArgumentException(
                "The password is empty or exceeds the maximum allowed length.",
                nameof(password));
        }
    }

    private bool VerifyCredentials(string identifier, string password)
    {
        bool isAuthenticated;

        try
        {
            bool isRegistered = _playerRepository.TryFindCredentials(identifier, out PlayerCredentials credentials);
            isAuthenticated = isRegistered && PasswordHasher.Verify(credentials.PasswordHash, password);
            LogLoginAttempt(isAuthenticated, credentials);
        }
        catch (DatabaseOperationException exception)
        {
            throw new DatabaseUnavailableException(
                "The player credentials could not be read from the database.",
                exception);
        }

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
