using DominoTrainGame.Server.Application.Ports;
using System.Text.RegularExpressions;
using DominoTrainGame.Utils;

namespace DominoTrainGame;

public sealed class SignUpValidator
{
    private readonly IAuthenticationStore _store;

    public SignUpValidator(IAuthenticationStore store)
    {
        _store = store;
    }

    private const int MinimumPasswordLength = 12;

    private static readonly Regex _emailPattern;

    static SignUpValidator()
    {
        _emailPattern = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    }

    public SignUpValidationStatus RegisterUser(string username, string email, string password)
    {
        SignUpValidationStatus finalStatus = ValidateInput(username, email, password);

        if (finalStatus == SignUpValidationStatus.Success)
        {
            finalStatus = _store.SaveNewPlayer(username, email, password);
        }

        return finalStatus;
    }

    public static SignUpValidationStatus ValidateInput(string username, string email, string password)
    {
        SignUpValidationStatus status = SignUpValidationStatus.Success;

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password))
        {
            status = SignUpValidationStatus.EmptyFields;
        }
        else if (!_emailPattern.IsMatch(email))
        {
            status = SignUpValidationStatus.InvalidEmail;
        }
        else if (password.Length < MinimumPasswordLength)
        {
            status = SignUpValidationStatus.PasswordTooShort;
        }

        return status;
    }

    public SignUpValidationStatus CheckAvailability(string username, string email, string password)
    {
        SignUpValidationStatus status = ValidateInput(username, email, password);

        if (status == SignUpValidationStatus.Success)
        {
            status = _store.FindExistingPlayer(username, email);
        }

        return status;
    }

}
