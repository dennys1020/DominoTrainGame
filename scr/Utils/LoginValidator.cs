using System;
using System.Data.Entity.Core;
using System.Linq;
using log4net.Ext.Trace;
using DominoTrainGame.Resources.Localization;

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
            using (DominoGameDBEntities dbContext = new DominoGameDBEntities())
            {
                Player user = dbContext.Players.FirstOrDefault(
                    p => p.UserName == usernameOrEmail || p.Email == usernameOrEmail);

                if (user == null)
                {
                    message = UiStrings.MessageUserNotFound;

                    return false;
                }

                bool isPasswordValid = PasswordHasher.Verify(user.PasswordHash, password);

                if (!isPasswordValid)
                {
                    message = UiStrings.MessageIncorrectPassword;

                    return false;
                }

                message = UiStrings.MessageLoginSuccess;

                return true;
            }
        }
        catch (EntityException exception)
        {
            _logger.Error(
                "The login operation failed due to a database exception.",
                exception);

            message = UiStrings.DatabaseErrorMessage;

            return false;
        }
    }
}