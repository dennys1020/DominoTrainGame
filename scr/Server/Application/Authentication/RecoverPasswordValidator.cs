using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Errors;
using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Application.Authentication;

public sealed class RecoverPasswordValidator
{
    public const int MinimumPasswordLength = 12;
    private readonly IAuthenticationStore _store;

    public RecoverPasswordValidator(IAuthenticationStore store) => _store = store;

    public RecoveryAccountResult FindAccount(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return new RecoveryAccountResult { Status = RecoverPasswordValidationStatus.EmptyFields };
        }
        try
        {
            AccountRecord account = _store.FindAccount(identifier);
            return account is null
                ? new RecoveryAccountResult { Status = RecoverPasswordValidationStatus.UserNotFound }
                : new RecoveryAccountResult
                {
                    Status = RecoverPasswordValidationStatus.Success,
                    PlayerId = account.PlayerId,
                    Email = account.Email
                };
        }
        catch (DatabaseOperationException)
        {
            return new RecoveryAccountResult { Status = RecoverPasswordValidationStatus.DatabaseError };
        }
    }

    public RecoverPasswordValidationStatus UpdatePassword(int playerId, string password, string confirmation)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmation))
        {
            return RecoverPasswordValidationStatus.EmptyFields;
        }
        if (password.Length < MinimumPasswordLength)
        {
            return RecoverPasswordValidationStatus.PasswordTooShort;
        }
        if (password != confirmation)
        {
            return RecoverPasswordValidationStatus.PasswordsDoNotMatch;
        }
        try
        {
            return _store.SaveNewPassword(playerId, PasswordHasher.Hash(password))
                ? RecoverPasswordValidationStatus.Success : RecoverPasswordValidationStatus.UserNotFound;
        }
        catch (DatabaseOperationException)
        {
            return RecoverPasswordValidationStatus.DatabaseError;
        }
    }
}
