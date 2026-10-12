using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Validator;

namespace DominoTrainGame;

public sealed class LoginValidator
{
    private readonly IAuthenticationStore _store;

    public LoginValidator(IAuthenticationStore store)
    {
        _store = store;
    }

    public LoginValidationStatus LogInUser(string usernameOrEmail, string password)
    {
        LoginValidationStatus status = ValidateInput(usernameOrEmail, password);

        if (status == LoginValidationStatus.Success)
        {
            status = _store.AuthenticateUser(usernameOrEmail, password);
        }

        return status;
    }

    private static LoginValidationStatus ValidateInput(string usernameOrEmail, string password)
    {
        LoginValidationStatus status = LoginValidationStatus.Success;

        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            status = LoginValidationStatus.EmptyFields;
        }

        return status;
    }

}
