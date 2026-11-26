using System;
using System.Net.Mail;

namespace DominoTrainGame.Client.ViewModel.Validation;

public static class SignUpInputValidator
{
    private const int MinimumPasswordLength = 8;

    public static SignUpInputStatus Validate(string username, string email, string password)
    {
        string normalizedUsername = InputText.Normalize(username);
        string normalizedEmail = InputText.Normalize(email);
        string enteredPassword = password ?? string.Empty;
        SignUpInputStatus inputStatus = SignUpInputStatus.Valid;

        if (HasEmptyField(normalizedUsername, normalizedEmail, enteredPassword))
        {
            inputStatus = SignUpInputStatus.EmptyFields;
        }
        else if (normalizedUsername.Length > PlayerInputLimits.MaximumUsernameLength)
        {
            inputStatus = SignUpInputStatus.UsernameTooLong;
        }
        else if (normalizedEmail.Length > PlayerInputLimits.MaximumEmailLength)
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

    private static bool HasEmptyField(string username, string email, string password)
    {
        bool isUsernameEmpty = username.Length == InputText.EmptyLength;
        bool isEmailEmpty = email.Length == InputText.EmptyLength;
        bool isPasswordEmpty = password.Length == InputText.EmptyLength;
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
        else if (password.Length > PlayerInputLimits.MaximumPasswordLength)
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
        catch (FormatException)
        {
            isWellFormed = false;
        }

        return isWellFormed;
    }
}
