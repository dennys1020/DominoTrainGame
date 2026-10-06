using System.Data.Entity.Core;
using System.Linq;
using log4net.Ext.Trace;
using DominoTrainGame.Validator;

namespace DominoTrainGame;

public sealed class LoginValidator
{
    private static readonly ITraceLog _logger;

    static LoginValidator()
    {
        _logger = TraceLogManager.GetLogger(typeof(LoginValidator));
    }

    public LoginValidationStatus LogInUser(string usernameOrEmail, string password)
    {
        LoginValidationStatus status = ValidateInput(usernameOrEmail, password);

        if (status == LoginValidationStatus.Success)
        {
            status = AuthenticateUser(usernameOrEmail, password);
        }

        return status;
    }

    private static LoginValidationStatus ValidateInput(string usernameOrEmail, string password)
    {
        LoginValidationStatus status = LoginValidationStatus.Success;

        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            status = LoginValidationStatus.EmptyFields;
        }

        return status;
    }

    private LoginValidationStatus AuthenticateUser(string usernameOrEmail, string password)
    {
        LoginValidationStatus status = LoginValidationStatus.Success;

        try
        {
            using (DominoGameDBEntities databaseContext = DatabaseContextFactory.Create())
            {
                Player user = databaseContext.Players.FirstOrDefault(
                    registeredPlayer => registeredPlayer.Username == usernameOrEmail
                        || registeredPlayer.Email == usernameOrEmail);

                if (user is null)
                {
                    status = LoginValidationStatus.UserNotFound;
                }
                else if (!PasswordHasher.Verify(user.PasswordHash, password))
                {
                    status = LoginValidationStatus.IncorrectPassword;
                }
            }
        }
        catch (EntityException exception)
        {
            _logger.Error(
                "The login operation failed due to a database exception.",
                exception);

            status = LoginValidationStatus.DatabaseError;
        }

        return status;
    }
}
