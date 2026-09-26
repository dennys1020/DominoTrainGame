using System;
using System.Linq;
using DominoTrainGame.Resources.Localization;
using log4net.Ext.Trace;

namespace DominoTrainGame;

public sealed class LoginValidator
{
    private static readonly ITraceLog _logger;

    static LoginValidator()
    {
        _logger = TraceLogManager.GetLogger(typeof(LoginValidator));
    }

    public bool TryLogin(string usernameOrEmail, string password, out string message)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            message = UiStrings.MessageRequiredFields;
            return false;
        }

        try
        {
            using (DominoTrainBDEntities dbContext = new DominoTrainBDEntities())
            {
                Players user = dbContext.Players.FirstOrDefault(
                    p => p.Username == usernameOrEmail || p.Email == usernameOrEmail);

                if (user == null)
                {
                    message = UiStrings.MessageUserNotFound;
                    return false;
                }

                string enteredHash = PasswordHasher.Hash(password);

                if (user.Password == enteredHash)
                {
                    message = UiStrings.MessageLoginSuccess;
                    return true;
                }

                message = UiStrings.MessageIncorrectPassword;
                return false;
            }
        }
        catch (Exception exception)
        {
            _logger.Error(
                "The login operation failed due to a database exception.",
                exception);

            message = UiStrings.DatabaseErrorMessage;
            return false;
        }
    }
}