using System;
using System.Net.Mail;
using DominoTrainGame.Shared.Common;
using DominoTrainGame.Shared.Rules;

namespace DominoTrainGame.Client.ViewModel.Validation;

public static class SignUpInputValidator
{

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
        else if (GameRules.IsUsernameTooLong(normalizedUsername))
        {
            inputStatus = SignUpInputStatus.UsernameTooLong;
        }
        else if (GameRules.IsEmailTooLong(normalizedEmail))
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

        if (GameRules.IsPasswordTooShort(password))
        {
            inputStatus = SignUpInputStatus.PasswordTooShort;
        }
        else if (GameRules.IsPasswordTooLong(password))
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
