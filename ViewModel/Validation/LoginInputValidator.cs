namespace DominoTrainGame.Client.ViewModel.Validation;

public static class LoginInputValidator
{
    public static LoginInputStatus Validate(string identifier, string password)
    {
        string normalizedIdentifier = InputText.Normalize(identifier);
        string enteredPassword = password ?? string.Empty;
        LoginInputStatus inputStatus = LoginInputStatus.Valid;

        if (normalizedIdentifier.Length == InputText.EmptyLength || enteredPassword.Length == InputText.EmptyLength)
        {
            inputStatus = LoginInputStatus.EmptyFields;
        }
        else if (normalizedIdentifier.Length > PlayerInputLimits.MaximumEmailLength)
        {
            inputStatus = LoginInputStatus.IdentifierTooLong;
        }
        else if (enteredPassword.Length > PlayerInputLimits.MaximumPasswordLength)
        {
            inputStatus = LoginInputStatus.PasswordTooLong;
        }

        return inputStatus;
    }
}
