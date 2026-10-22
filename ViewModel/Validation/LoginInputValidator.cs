namespace DominoTrainGame.Client.ViewModel.Validation;

public static class LoginInputValidator
{
    private const int MaximumIdentifierLength = 255;
    private const int MaximumPasswordLength = 128;
    private const int EmptyLength = 0;

    public static LoginInputStatus Validate(string identifier, string password)
    {
        string normalizedIdentifier = identifier?.Trim() ?? string.Empty;
        string enteredPassword = password ?? string.Empty;
        LoginInputStatus inputStatus = LoginInputStatus.Valid;

        if (normalizedIdentifier.Length == EmptyLength || enteredPassword.Length == EmptyLength)
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
