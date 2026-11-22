using System;
using System.Net.Mail;
using log4net.Ext.Trace;

namespace DominoTrainGame.Client.ViewModel.Validation;

public static class SignUpInputValidator
{
    private const int MaximumUsernameLength = 45;
    private const int MaximumEmailLength = 255;
    private const int MinimumPasswordLength = 8;
    private const int MaximumPasswordLength = 128;
    private const int EmptyLength = 0;

    private static readonly ITraceLog _logger;

    static SignUpInputValidator()
    {
        _logger = TraceLogManager.GetLogger(typeof(SignUpInputValidator));
    }

    public static SignUpInputStatus Validate(string username, string email, string password)
    {
        string normalizedUsername = NormalizeIdentifier(username);
        string normalizedEmail = NormalizeIdentifier(email);
        string enteredPassword = password ?? string.Empty;
        SignUpInputStatus inputStatus = SignUpInputStatus.Valid;

        if (HasEmptyField(normalizedUsername, normalizedEmail, enteredPassword))
        {
            inputStatus = SignUpInputStatus.EmptyFields;
        }
        else if (normalizedUsername.Length > MaximumUsernameLength)
        {
            inputStatus = SignUpInputStatus.UsernameTooLong;
        }
        else if (normalizedEmail.Length > MaximumEmailLength)
        {
            inputStatus = SignUpInputStatus.EmailTooLong;
        }
        else if (!IsWellFormedEmail(normalizedEmail))
        {
            inputStatus = SignUpInputStatus.EmailInvalid;
        }
        else
        {
            inputStatus = ValidatePasswordLength(enteredPassword);
        }

        return inputStatus;
    }

    private static string NormalizeIdentifier(string identifier)
    {
        string normalizedIdentifier = identifier?.Trim() ?? string.Empty;
        return normalizedIdentifier;
    }

    private static bool HasEmptyField(string username, string email, string password)
    {
        bool isUsernameEmpty = username.Length == EmptyLength;
        bool isEmailEmpty = email.Length == EmptyLength;
        bool isPasswordEmpty = password.Length == EmptyLength;
        bool hasEmptyField = isUsernameEmpty || isEmailEmpty || isPasswordEmpty;
        return hasEmptyField;
    }

    private static SignUpInputStatus ValidatePasswordLength(string password)
    {
        SignUpInputStatus inputStatus = SignUpInputStatus.Valid;

        if (password.Length < MinimumPasswordLength)
        {
            inputStatus = SignUpInputStatus.PasswordTooShort;
        }
        else if (password.Length > MaximumPasswordLength)
        {
            inputStatus = SignUpInputStatus.PasswordTooLong;
        }

        return inputStatus;
    }

    private static bool IsWellFormedEmail(string email)
    {
        bool isWellFormed;

        try
        {
            MailAddress mailAddress = new MailAddress(email);
            isWellFormed = string.Equals(mailAddress.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException exception)
        {
            _logger.Debug("An email address was rejected because its format is invalid.", exception);
            isWellFormed = false;
        }

        return isWellFormed;
    }
}
