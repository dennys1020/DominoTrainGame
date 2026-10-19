namespace DominoTrainGame.ViewModels.Validation;

public static class LoginInputValidator
{
    private const int MaximumIdentifierLength = 255;
    private const int MaximumPasswordLength = 128;

    public static LoginInputStatus Validate(string identifier, string password)
    {
        string normalizedIdentifier = identifier?.Trim() ?? string.Empty;
        string enteredPassword = password ?? string.Empty;
        LoginInputStatus inputStatus = LoginInputStatus.Valid;

        if (normalizedIdentifier.Length == 0 || enteredPassword.Length == 0)
        {
            inputStatus = LoginInputStatus.EmptyFields;
        }
        else if (normalizedIdentifier.Length > MaximumIdentifierLength)
        {
            inputStatus = LoginInputStatus.IdentifierTooLong;
        }
        else if (enteredPassword.Length > MaximumPasswordLength)
        {
            inputStatus = LoginInputStatus.PasswordTooLong;
        }

        return inputStatus;
    }
}
