using System;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Persistence.Contracts;
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

    public LoginResult LogIn(string identifier, string password)
    {
        LoginResult loginResult = LoginResult.InvalidInput;
        string normalizedIdentifier = identifier?.Trim() ?? string.Empty;

        if (IsValidInput(normalizedIdentifier, password))
        {
            loginResult = VerifyCredentials(normalizedIdentifier, password);
        }

        return loginResult;
    }

    private static bool IsValidInput(string identifier, string password)
    {
        bool isIdentifierValid = !string.IsNullOrWhiteSpace(identifier)
            && identifier.Length <= MaximumIdentifierLength;
        bool isPasswordValid = !string.IsNullOrEmpty(password)
            && password.Length <= MaximumPasswordLength;
        return isIdentifierValid && isPasswordValid;
    }

    private LoginResult VerifyCredentials(string identifier, string password)
    {
        LoginResult loginResult = LoginResult.DatabaseUnavailable;

        try
        {
            bool isRegistered = _playerRepository.TryFindCredentials(identifier, out PlayerCredentials credentials);
            bool isPasswordCorrect = isRegistered && PasswordHasher.Verify(credentials.PasswordHash, password);

            loginResult = isPasswordCorrect ? LoginResult.Success : LoginResult.InvalidCredentials;
            LogLoginAttempt(loginResult, credentials);
        }
        catch (DatabaseOperationException exception)
        {
            _logger.Error("The player credentials could not be read from the database.", exception);
        }

        return loginResult;
    }

    private static void LogLoginAttempt(LoginResult loginResult, PlayerCredentials credentials)
    {
        if (loginResult == LoginResult.Success)
        {
            _logger.InfoFormat("A player logged in. PlayerId: {0}.", credentials.PlayerId);
        }
        else
        {
            _logger.Info("A login attempt was rejected because the credentials are invalid.");
        }
    }
}
