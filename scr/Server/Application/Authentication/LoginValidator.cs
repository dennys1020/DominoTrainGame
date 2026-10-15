using System;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Errors;
using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Application.Authentication;

public sealed class LoginValidator
{
    private readonly IAuthenticationStore _store;

    public LoginValidator(IAuthenticationStore store)
    {
        _store = store
            ?? throw new ArgumentNullException(nameof(store));
    }

    public LoginValidationStatus LogInUser(string identifier, string password)
    {
        LoginValidationStatus status;

        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
        {
            status = LoginValidationStatus.EmptyFields;
        }
        else
        {
            try
            {
                AccountRecord account = _store.FindAccount(identifier);

                if (account is null)
                {
                    status = LoginValidationStatus.UserNotFound;
                }
                else
                {
                    status = PasswordHasher.Verify(account.PasswordHash, password)
                        ? LoginValidationStatus.Success
                        : LoginValidationStatus.IncorrectPassword;
                }
            }
            catch (DatabaseOperationException)
            {
                status = LoginValidationStatus.DatabaseError;
            }
        }

        return status;
    }
}
