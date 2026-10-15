using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Errors;
using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Application.Authentication;

public sealed class LoginValidator
{
    private readonly IAuthenticationStore _store;

    public LoginValidator(IAuthenticationStore store) => _store = store;

    public LoginValidationStatus LogInUser(string identifier, string password)
    {
        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
        {
            return LoginValidationStatus.EmptyFields;
        }

        try
        {
            AccountRecord account = _store.FindAccount(identifier);
            if (account is null)
            {
                return LoginValidationStatus.UserNotFound;
            }

            return PasswordHasher.Verify(account.PasswordHash, password)
                ? LoginValidationStatus.Success : LoginValidationStatus.IncorrectPassword;
        }
        catch (DatabaseOperationException)
        {
            return LoginValidationStatus.DatabaseError;
        }
    }
}
