using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Validation;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class SignUpMessageKeys
{
    private const string UsernameTooLongMessageKey = "UsernameTooLongMessage";
    private const string EmailTooLongMessageKey = "EmailTooLongMessage";
    private const string EmailInvalidMessageKey = "EmailInvalidMessage";
    private const string PasswordTooShortMessageKey = "PasswordTooShortMessage";
    private const string AccountAlreadyExistsMessageKey = "AccountAlreadyExistsMessage";
    private const string SignUpFailedMessageKey = "SignUpFailedMessage";
    private const string SignUpSystemErrorMessageKey = "SignUpSystemErrorMessage";
    private const string EmailUnavailableMessageKey = "EmailUnavailableMessage";
    private const string InvalidVerificationCodeMessageKey = "InvalidVerificationCodeMessage";
    private const string VerificationCodeExpiredMessageKey = "VerificationCodeExpiredMessage";
    private const string TooManyVerificationAttemptsMessageKey = "TooManyVerificationAttemptsMessage";
    private const string VerificationNotFoundMessageKey = "VerificationNotFoundMessage";
    private const string ResendTooSoonMessageKey = "VerificationResendTooSoonMessage";

    public static string GetInputMessageKey(SignUpInputStatus inputStatus)
    {
        string messageKey;

        switch (inputStatus)
        {
            case SignUpInputStatus.UsernameTooLong:
                messageKey = UsernameTooLongMessageKey;
                break;
            case SignUpInputStatus.EmailTooLong:
                messageKey = EmailTooLongMessageKey;
                break;
            case SignUpInputStatus.EmailInvalid:
                messageKey = EmailInvalidMessageKey;
                break;
            case SignUpInputStatus.PasswordTooShort:
                messageKey = PasswordTooShortMessageKey;
                break;
            case SignUpInputStatus.PasswordTooLong:
                messageKey = CommonMessageKeys.PasswordTooLongMessageKey;
                break;
            default:
                messageKey = CommonMessageKeys.RequiredFieldsMessageKey;
                break;
        }

        return messageKey;
    }

    public static string GetResultMessageKey(SignUpResult signUpResult)
    {
        string messageKey;

        switch (signUpResult)
        {
            case SignUpResult.AlreadyExists:
                messageKey = AccountAlreadyExistsMessageKey;
                break;
            case SignUpResult.InvalidInput:
                messageKey = SignUpFailedMessageKey;
                break;
            case SignUpResult.ServerUnavailable:
                messageKey = CommonMessageKeys.ServerUnavailableMessageKey;
                break;
            case SignUpResult.DatabaseUnavailable:
                messageKey = CommonMessageKeys.DatabaseErrorMessageKey;
                break;
            case SignUpResult.EmailUnavailable:
                messageKey = EmailUnavailableMessageKey;
                break;
            default:
                messageKey = SignUpSystemErrorMessageKey;
                break;
        }

        return messageKey;
    }

    public static string GetVerificationMessageKey(EmailVerificationResult verificationResult)
    {
        string messageKey;

        switch (verificationResult)
        {
            case EmailVerificationResult.InvalidCode:
                messageKey = InvalidVerificationCodeMessageKey;
                break;
            case EmailVerificationResult.CodeExpired:
                messageKey = VerificationCodeExpiredMessageKey;
                break;
            case EmailVerificationResult.TooManyAttempts:
                messageKey = TooManyVerificationAttemptsMessageKey;
                break;
            case EmailVerificationResult.EmailUnavailable:
                messageKey = EmailUnavailableMessageKey;
                break;
            case EmailVerificationResult.RegistrationNotFound:
                messageKey = VerificationNotFoundMessageKey;
                break;
            case EmailVerificationResult.ResendTooSoon:
                messageKey = ResendTooSoonMessageKey;
                break;
            case EmailVerificationResult.AlreadyExists:
                messageKey = AccountAlreadyExistsMessageKey;
                break;
            case EmailVerificationResult.ServerUnavailable:
                messageKey = CommonMessageKeys.ServerUnavailableMessageKey;
                break;
            case EmailVerificationResult.DatabaseUnavailable:
                messageKey = CommonMessageKeys.DatabaseErrorMessageKey;
                break;
            default:
                messageKey = SignUpSystemErrorMessageKey;
                break;
        }

        return messageKey;
    }
}
