using System;
using System.Net.Mail;

namespace DominoTrainGame.Client.ViewModel.Validation;

public static class SignUpInputValidator
{
    private const int MaximumUsernameLength = 45;
    private const int MaximumEmailLength = 255;
    private const int MinimumPasswordLength = 8;
    private const int MaximumPasswordLength = 128;
    private const int EmptyLength = 0;

    public static SignUpInputStatus Validate(string username, string email, string password)
    {
        string normalizedUsername = username?.Trim() ?? string.Empty;
        string normalizedEmail = email?.Trim() ?? string.Empty;
        string enteredPassword = password ?? string.Empty;
        SignUpInputStatus inputStatus = SignUpInputStatus.Valid;

        bool isUsernameEmpty = normalizedUsername.Length == EmptyLength;
        bool isEmailEmpty = normalizedEmail.Length == EmptyLength;
        bool isPasswordEmpty = enteredPassword.Length == EmptyLength;

        if (isUsernameEmpty || isEmailEmpty || isPasswordEmpty)
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
        else if (enteredPassword.Length < MinimumPasswordLength)
        {
            inputStatus = SignUpInputStatus.PasswordTooShort;
        }
        else if (enteredPassword.Length > MaximumPasswordLength)
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
            MailAddress address = new MailAddress(email);
            isWellFormed = string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            isWellFormed = false;
        }

        return isWellFormed;
    }
}
