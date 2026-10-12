using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using log4net.Ext.Trace;
using DominoTrainGame.Utils;
using DominoTrainGame.Validator;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Infrastructure;

public sealed class SqlAuthenticationStore : IAuthenticationStore
{
    private const byte NotGuest = 0;
    private static readonly ITraceLog _logger = TraceLogManager.GetLogger(typeof(SqlAuthenticationStore));

    public LoginValidationStatus AuthenticateUser(string usernameOrEmail, string password)
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

    public SignUpValidationStatus SaveNewPlayer(string username, string email, string password)
    {
        SignUpValidationStatus status = SignUpValidationStatus.Success;

        try
        {
            using (DominoGameDBEntities databaseContext = DatabaseContextFactory.Create())
            {
                bool isAlreadyRegistered = databaseContext.Players.Any(
                    registeredPlayer => registeredPlayer.Username == username || registeredPlayer.Email == email);

                if (isAlreadyRegistered)
                {
                    _logger.Warn("The sign up was rejected because the username or email is already registered.");
                    status = SignUpValidationStatus.UserAlreadyExists;
                }
                else
                {
                    DateTime registrationTime = DateTime.UtcNow;

                    Player newPlayer = new Player
                    {
                        Username = username,
                        Email = email,
                        PasswordHash = PasswordHasher.Hash(password),
                        IsGuest = NotGuest
                    };

                    Account newAccount = new Account
                    {
                        Biography = string.Empty,
                        PreferredLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                        CreatedAt = registrationTime,
                        LastSeenAt = registrationTime
                    };

                    newPlayer.Accounts.Add(newAccount);

                    databaseContext.Players.Add(newPlayer);
                    databaseContext.SaveChanges();

                    _logger.Info("A new player was registered.");
                }
            }
        }
        catch (DbEntityValidationException exception)
        {
            _logger.Error(SignUpValidatorBuildValidationFailureMessage(exception), exception);
            status = SignUpValidationStatus.DatabaseError;
        }
        catch (DbUpdateException exception)
        {
            _logger.Error("The new player could not be saved.", exception);
            status = SignUpValidationStatus.DatabaseError;
        }
        catch (EntityException exception)
        {
            _logger.Error("The database could not be reached while registering the player.", exception);
            status = SignUpValidationStatus.DatabaseError;
        }

        return status;
    }

    public SignUpValidationStatus FindExistingPlayer(string username, string email)
    {
        SignUpValidationStatus status = SignUpValidationStatus.Success;

        try
        {
            using (DominoGameDBEntities databaseContext = DatabaseContextFactory.Create())
            {
                bool isAlreadyRegistered = databaseContext.Players.Any(
                    registeredPlayer => registeredPlayer.Username == username || registeredPlayer.Email == email);

                if (isAlreadyRegistered)
                {
                    status = SignUpValidationStatus.UserAlreadyExists;
                }
            }
        }
        catch (EntityException exception)
        {
            _logger.Error("The database could not be reached while checking the player.", exception);
            status = SignUpValidationStatus.DatabaseError;
        }

        return status;
    }

    private static string SignUpValidatorBuildValidationFailureMessage(DbEntityValidationException exception)
    {
        string failedProperties = string.Join(", ",
            exception.EntityValidationErrors
                .SelectMany(validationResult => validationResult.ValidationErrors)
                .Select(validationError => validationError.PropertyName));

        string message = string.Format(
            "The new player failed entity validation. Properties: {0}.",
            failedProperties);

        return message;
    }

    public RecoverPasswordValidationStatus LookUpAccount(string identifier, out int playerId, out string email)
    {
        playerId = 0;
        email = string.Empty;
        RecoverPasswordValidationStatus status = RecoverPasswordValidationStatus.Success;

        try
        {
            using (DominoGameDBEntities databaseContext = DatabaseContextFactory.Create())
            {
                Player foundPlayer = databaseContext.Players.FirstOrDefault(
                    registeredPlayer => registeredPlayer.Username == identifier
                        || registeredPlayer.Email == identifier);

                if (foundPlayer is null)
                {
                    status = RecoverPasswordValidationStatus.UserNotFound;
                }
                else
                {
                    playerId = foundPlayer.PlayerId;
                    email = foundPlayer.Email;
                }
            }
        }
        catch (EntityException exception)
        {
            _logger.Error("The database could not be reached while looking up the account.", exception);
            status = RecoverPasswordValidationStatus.DatabaseError;
        }

        return status;
    }

    public RecoverPasswordValidationStatus SaveNewPassword(int playerId, string newPassword)
    {
        RecoverPasswordValidationStatus status = RecoverPasswordValidationStatus.Success;

        try
        {
            using (DominoGameDBEntities databaseContext = DatabaseContextFactory.Create())
            {
                Player player = databaseContext.Players.FirstOrDefault(
                    registeredPlayer => registeredPlayer.PlayerId == playerId);

                if (player is null)
                {
                    status = RecoverPasswordValidationStatus.UserNotFound;
                }
                else
                {
                    player.PasswordHash = PasswordHasher.Hash(newPassword);
                    databaseContext.SaveChanges();

                    _logger.Info("A player's password was recovered and updated.");
                }
            }
        }
        catch (DbEntityValidationException exception)
        {
            _logger.Error(RecoverPasswordValidatorBuildValidationFailureMessage(exception), exception);
            status = RecoverPasswordValidationStatus.DatabaseError;
        }
        catch (DbUpdateException exception)
        {
            _logger.Error("The new password could not be saved.", exception);
            status = RecoverPasswordValidationStatus.DatabaseError;
        }
        catch (EntityException exception)
        {
            _logger.Error("The database could not be reached while updating the password.", exception);
            status = RecoverPasswordValidationStatus.DatabaseError;
        }

        return status;
    }

    private static string RecoverPasswordValidatorBuildValidationFailureMessage(DbEntityValidationException exception)
    {
        string failedProperties = string.Join(", ",
            exception.EntityValidationErrors
                .SelectMany(validationResult => validationResult.ValidationErrors)
                .Select(validationError => validationError.PropertyName));

        return string.Format(
            "The password update failed entity validation. Properties: {0}.",
            failedProperties);
    }
}
