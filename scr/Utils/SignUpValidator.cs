using System;
using System.Windows.Controls;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using log4net.Ext.Trace;
using DominoTrainGame.Utils;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame;

public sealed class SignUpValidator
{
    private const int MinimumPasswordLength = 12;
    private const byte NotGuest = 0;
    private static readonly ITraceLog _logger;
    private static readonly Regex _emailPattern;

    static SignUpValidator()
    {
        _logger = TraceLogManager.GetLogger(typeof(SignUpValidator));
        _emailPattern = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    }

    public SignUpValidationStatus RegisterUser(string username, string email, string password)
    {
        SignUpValidationStatus finalStatus = ValidateInput(username, email, password);

        if (finalStatus == SignUpValidationStatus.Success)
        {
            finalStatus = SaveNewPlayer(username, email, password);
        }

        return finalStatus;
    }

    public static SignUpValidationStatus ValidateInput(string username, string email, string password)
    {
        SignUpValidationStatus status = SignUpValidationStatus.Success;

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password))
        {
            status = SignUpValidationStatus.EmptyFields;
        }
        else if (!_emailPattern.IsMatch(email))
        {
            status = SignUpValidationStatus.InvalidEmail;
        }
        else if (password.Length < MinimumPasswordLength)
        {
            status = SignUpValidationStatus.PasswordTooShort;
        }

        return status;
    }

    private static string BuildValidationFailureMessage(DbEntityValidationException exception)
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

    private SignUpValidationStatus SaveNewPlayer(string username, string email, string password)
    {
        SignUpValidationStatus status = SignUpValidationStatus.Success;

        try
        {
            using (DominoGameDBEntities databaseContext = new DominoGameDBEntities())
            {
                bool isAlreadyRegistered = databaseContext.Players.Any(
                    registeredPlayer => registeredPlayer.userName == username || registeredPlayer.Email == email);

                if (isAlreadyRegistered)
                {
                    _logger.Warn("The sign up was rejected because the username or email is already registered.");
                    status = SignUpValidationStatus.UserAlreadyExists;
                }
                else
                {
                    Player newPlayer = new Player
                    {
                        userName = username,
                        Email = email,
                        PasswordHash = PasswordHasher.Hash(password),
                        PreferredLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                        CreatedAt = DateTime.UtcNow,
                        IsGuest = NotGuest
                    };

                    databaseContext.Players.Add(newPlayer);
                    databaseContext.SaveChanges();

                    _logger.Info("A new player was registered.");
                }
            }
        }
        catch (DbEntityValidationException exception)
        {
            _logger.Error(BuildValidationFailureMessage(exception), exception);
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
    public SignUpValidationStatus CheckAvailability(string username, string email, string password)
    {
        SignUpValidationStatus status = ValidateInput(username, email, password);

        if (status == SignUpValidationStatus.Success)
        {
            status = FindExistingPlayer(username, email);
        }

        return status;
    }

    private SignUpValidationStatus FindExistingPlayer(string username, string email)
    {
        SignUpValidationStatus status = SignUpValidationStatus.Success;

        try
        {
            using (DominoGameDBEntities databaseContext = new DominoGameDBEntities())
            {
                bool isAlreadyRegistered = databaseContext.Players.Any(
                    registeredPlayer => registeredPlayer.userName == username || registeredPlayer.Email == email);

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

}