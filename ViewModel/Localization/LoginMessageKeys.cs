using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Validation;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class LoginMessageKeys
{
    private const string IdentifierTooLongMessageKey = "IdentifierTooLongMessage";
    private const string InvalidCredentialsMessageKey = "InvalidCredentialsMessage";
    private const string LoginFailedMessageKey = "LoginFailedMessage";
    private const string LoginSystemErrorMessageKey = "LoginSystemErrorMessage";
    private const string AlreadyConnectedMessageKey = "AlreadyConnectedMessage";

    public static string GetInputMessageKey(LoginInputStatus inputStatus)
    {
        string messageKey;

        switch (inputStatus)
        {
            case LoginInputStatus.IdentifierTooLong:
                messageKey = IdentifierTooLongMessageKey;
                break;
            case LoginInputStatus.PasswordTooLong:
                messageKey = CommonMessageKeys.PasswordTooLongMessageKey;
                break;
            default:
                messageKey = CommonMessageKeys.RequiredFieldsMessageKey;
                break;
        }

        return messageKey;
    }

    public static string GetResultMessageKey(LoginResult loginResult)
    {
        string messageKey;

        switch (loginResult)
        {
            case LoginResult.InvalidCredentials:
                messageKey = InvalidCredentialsMessageKey;
                break;
            case LoginResult.InvalidInput:
                messageKey = LoginFailedMessageKey;
                break;
            case LoginResult.AlreadyConnected:
                messageKey = AlreadyConnectedMessageKey;
                break;
            case LoginResult.ServerUnavailable:
                messageKey = CommonMessageKeys.ServerUnavailableMessageKey;
                break;
            case LoginResult.DatabaseUnavailable:
                messageKey = CommonMessageKeys.DatabaseErrorMessageKey;
                break;
            default:
                messageKey = LoginSystemErrorMessageKey;
                break;
        }

        return messageKey;
    }

    public static string GetConnectionMessageKey(ConnectionCheckResult connectionResult)
    {
        string messageKey;

        switch (connectionResult)
        {
            case ConnectionCheckResult.ServerUnavailable:
                messageKey = CommonMessageKeys.ServerUnavailableMessageKey;
                break;
            case ConnectionCheckResult.DatabaseUnavailable:
                messageKey = CommonMessageKeys.DatabaseErrorMessageKey;
                break;
            default:
                messageKey = LoginSystemErrorMessageKey;
                break;
        }

        return messageKey;
    }
}
