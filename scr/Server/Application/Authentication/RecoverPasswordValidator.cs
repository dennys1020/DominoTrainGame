using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame;

public sealed class RecoverPasswordValidator
{
    private readonly IAuthenticationStore _store;

    public RecoverPasswordValidator(IAuthenticationStore store)
    {
        _store = store;
    }

    public const int MinimumPasswordLength = 12;

    public RecoverPasswordValidationStatus FindAccount(string identifier, out int playerId, out string email)
    {
        playerId = 0;
        email = string.Empty;

        RecoverPasswordValidationStatus status = ValidateIdentifier(identifier);

        if (status == RecoverPasswordValidationStatus.Success)
        {
            status = _store.LookUpAccount(identifier, out playerId, out email);
        }

        return status;
    }

    public RecoverPasswordValidationStatus UpdatePassword(int playerId, string newPassword, string confirmPassword)
    {
        RecoverPasswordValidationStatus status = ValidatePasswords(newPassword, confirmPassword);

        if (status == RecoverPasswordValidationStatus.Success)
        {
            status = _store.SaveNewPassword(playerId, newPassword);
        }

        return status;
    }

    private static RecoverPasswordValidationStatus ValidateIdentifier(string identifier)
    {
        return string.IsNullOrWhiteSpace(identifier)
            ? RecoverPasswordValidationStatus.EmptyFields
            : RecoverPasswordValidationStatus.Success;
    }

    private static RecoverPasswordValidationStatus ValidatePasswords(string newPassword, string confirmPassword)
    {
        RecoverPasswordValidationStatus status = RecoverPasswordValidationStatus.Success;

        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            status = RecoverPasswordValidationStatus.EmptyFields;
        }
        else if (newPassword.Length < MinimumPasswordLength)
        {
            status = RecoverPasswordValidationStatus.PasswordTooShort;
        }
        else if (newPassword != confirmPassword)
        {
            status = RecoverPasswordValidationStatus.PasswordsDoNotMatch;
        }

        return status;
    }

}
