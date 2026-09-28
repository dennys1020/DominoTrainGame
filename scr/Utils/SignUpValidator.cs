using DominoTrainGame.Utils;
using log4net.Ext.Trace;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;

namespace DominoTrainGame;

public sealed class SignUpValidator
{
    private const int MinimumPasswordLength = 12;

    private static readonly ITraceLog _logger;

    private static readonly Regex EmailPattern = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled);

    static SignUpValidator()
    {
        _logger = TraceLogManager.GetLogger(typeof(SignUpValidator));
    }

    public SignUpValidationStatus TryRegisterUser(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return SignUpValidationStatus.EmptyFields;
        }

        if (!EmailPattern.IsMatch(email))
        {
            return SignUpValidationStatus.InvalidEmail;
        }

        if (password.Length < MinimumPasswordLength)
        {
            return SignUpValidationStatus.PasswordTooShort;
        }

        try
        {
            using (DominoGameDBEntities databaseContext = new DominoGameDBEntities())
            {
                bool isUserExisting = databaseContext.Players.Any(p => p.userName == username || p.Email == email);

                if (isUserExisting)
                {
                    _logger.Warn("Registration attempt failed because the username or email was already taken.");
                    return SignUpValidationStatus.UserAlreadyExists;
                }

                Player newPlayer = new Player
                {
                    userName = username,
                    Email = email,
                    PasswordHash = PasswordHasher.Hash(password),
                    PreferredLanguage = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                    CreatedAt = System.DateTime.UtcNow,
                    IsGuest = 0
                };

                databaseContext.Players.Add(newPlayer);
                databaseContext.SaveChanges();

                _logger.Info("A new player registered successfully.");
                return SignUpValidationStatus.Success;
            }
        }
        catch (DbUpdateException exception)
        {
            _logger.Error("The registration operation failed while saving the new player.", exception);
            return SignUpValidationStatus.DatabaseError;
        }
        catch (EntityException exception)
        {
            _logger.Error("The registration operation failed because the database could not be reached.", exception);
            return SignUpValidationStatus.DatabaseError;
        }
    }
}
