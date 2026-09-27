using System;
using System.Data.Entity.Core;
using System.Linq;
using log4net.Ext.Trace;
using DominoTrainGame.Resources.Localization;

namespace DominoTrainGame;

/// <summary>
/// Validates credentials and signs in existing players.
/// </summary>
public sealed class SignUpValidator
{
    private static readonly ITraceLog _logger;

    static SignUpValidator()
    {
        _logger = TraceLogManager.GetLogger(typeof(SignUpValidator));
    }

    /// <summary>
    /// Attempts to sign in a player using a user name or email and a password.
    /// </summary>
    /// <param name="userNameOrEmail">The user name or email entered by the player.</param>
    /// <param name="password">The password entered by the player.</param>
    /// <param name="message">The user-facing result message.</param>
    /// <returns><see langword="true"/> when the credentials are valid.</returns>
    public bool TrySignIn(string userNameOrEmail, string password, out string message)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            message = UiStrings.MessageRequiredFields;

            return false;
        }

        try
        {
            using (DominoGameDBEntities dbContext = new DominoGameDBEntities())
            {
                Player existingUser = dbContext.Players.FirstOrDefault(
                    p => p.UserName == userNameOrEmail || p.Email == userNameOrEmail);

                if (existingUser == null)
                {
                    message = UiStrings.MessageUserNotFound;

                    return false;
                }

                bool isPasswordValid = PasswordHasher.Verify(
                    existingUser.PasswordHash,
                    password);

                if (!isPasswordValid)
                {
                    message = UiStrings.MessageIncorrectPassword;

                    return false;
                }

                message = UiStrings.MessageSignUpSuccess;

                return true;
            }
        }
        catch (EntityException exception)
        {
            _logger.Error(
                "The sign in operation failed due to a database exception.",
                exception);

            message = UiStrings.DatabaseErrorMessage;

            return false;
        }
    }
}