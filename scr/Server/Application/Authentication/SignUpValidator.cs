using System.Text.RegularExpressions;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Errors;

namespace DominoTrainGame.Server.Application.Authentication;

public sealed class SignUpValidator
{
    public const int MinimumPasswordLength = 12;
    private static readonly Regex _emailPattern = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private readonly IAuthenticationStore _store;

    public SignUpValidator(IAuthenticationStore store) => _store = store;

    public static SignUpValidationStatus ValidateInput(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password))
        {
            return SignUpValidationStatus.EmptyFields;
        }
        if (!_emailPattern.IsMatch(email))
        {
            return SignUpValidationStatus.InvalidEmail;
        }
        return password.Length < MinimumPasswordLength
            ? SignUpValidationStatus.PasswordTooShort : SignUpValidationStatus.Success;
    }

    public SignUpValidationStatus CheckAvailability(string username, string email, string password)
    {
        SignUpValidationStatus status = ValidateInput(username, email, password);
        if (status != SignUpValidationStatus.Success)
        {
            return status;
        }
        try
        {
            return _store.PlayerExists(username, email)
                ? SignUpValidationStatus.UserAlreadyExists : SignUpValidationStatus.Success;
        }
        catch (DatabaseOperationException)
        {
            return SignUpValidationStatus.DatabaseError;
        }
    }

    public SignUpValidationStatus RegisterUser(string username, string email, string password, string language)
    {
        SignUpValidationStatus status = ValidateInput(username, email, password);
        if (status != SignUpValidationStatus.Success)
        {
            return status;
        }
        try
        {
            return _store.SaveNewPlayer(username, email, PasswordHasher.Hash(password), language)
                ? SignUpValidationStatus.Success : SignUpValidationStatus.UserAlreadyExists;
        }
        catch (DatabaseOperationException)
        {
            return SignUpValidationStatus.DatabaseError;
        }
    }
}
