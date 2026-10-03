using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using log4net.Ext.Trace;
using DominoTrainGame.Utils;

namespace DominoTrainGame;

public sealed class RecoverPasswordValidator
{
    public const int MinimumPasswordLength = 12;

    private static readonly ITraceLog _logger;

    static RecoverPasswordValidator()
    {
        _logger = TraceLogManager.GetLogger(typeof(RecoverPasswordValidator));
    }

    public RecoverPasswordValidationStatus FindAccount(string identifier, out int playerId, out string email)
    {
        playerId = 0;
        email = string.Empty;

        RecoverPasswordValidationStatus status = ValidateIdentifier(identifier);

        if (status == RecoverPasswordValidationStatus.Success)
        {
            status = LookUpAccount(identifier, out playerId, out email);
        }

        return status;
    }

    public RecoverPasswordValidationStatus UpdatePassword(int playerId, string newPassword, string confirmPassword)
    {
        RecoverPasswordValidationStatus status = ValidatePasswords(newPassword, confirmPassword);

        if (status == RecoverPasswordValidationStatus.Success)
        {
            status = SaveNewPassword(playerId, newPassword);
        }

        return status;
    }

    private static RecoverPasswordValidationStatus ValidateIdentifier(string identifier)
    {
        return string.IsNullOrWhiteSpace(identifier)
            ? RecoverPasswordValidationStatus.EmptyFields
            : RecoverPasswordValidationStatus.Success;
    }

    private static RecoverPasswordValidationStatus ValidatePasswords(string newPassword, string confirmPassword)
    {
        RecoverPasswordValidationStatus status = RecoverPasswordValidationStatus.Success;

        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            status = RecoverPasswordValidationStatus.EmptyFields;
        }
        else if (newPassword.Length < MinimumPasswordLength)
        {
            status = RecoverPasswordValidationStatus.PasswordTooShort;
        }
        else if (newPassword != confirmPassword)
        {
            status = RecoverPasswordValidationStatus.PasswordsDoNotMatch;
        }

        return status;
    }

    private RecoverPasswordValidationStatus LookUpAccount(string identifier, out int playerId, out string email)
    {
        playerId = 0;
        email = string.Empty;
        RecoverPasswordValidationStatus status = RecoverPasswordValidationStatus.Success;

        try
        {
            using (DominoGameDBEntities databaseContext = DatabaseContextFactory.Create())
            {
                Player foundPlayer = databaseContext.Players.FirstOrDefault(
                    registeredPlayer => registeredPlayer.userName == identifier || registeredPlayer.Email == identifier);

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

    private RecoverPasswordValidationStatus SaveNewPassword(int playerId, string newPassword)
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
            _logger.Error(BuildValidationFailureMessage(exception), exception);
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

    private static string BuildValidationFailureMessage(DbEntityValidationException exception)
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
