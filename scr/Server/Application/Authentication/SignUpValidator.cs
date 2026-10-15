using System;
using System.Text.RegularExpressions;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Errors;

namespace DominoTrainGame.Server.Application.Authentication;

public sealed class SignUpValidator
{
    public const int MinimumPasswordLength = 12;

    private static readonly Regex _emailPattern = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled);

    private readonly IAuthenticationStore _store;

    public SignUpValidator(IAuthenticationStore store)
    {
        _store = store
            ?? throw new ArgumentNullException(nameof(store));
    }

    public static SignUpValidationStatus ValidateInput(
        string username,
        string email,
        string password)
    {
        SignUpValidationStatus status;

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
        else
        {
            status = SignUpValidationStatus.Success;
        }

        return status;
    }

    public SignUpValidationStatus CheckAvailability(
        string username,
        string email,
        string password)
    {
        SignUpValidationStatus status =
            ValidateInput(username, email, password);

        if (status == SignUpValidationStatus.Success)
        {
            try
            {
                bool playerExists =
                    _store.PlayerExists(username, email);

                status = playerExists
                    ? SignUpValidationStatus.UserAlreadyExists
                    : SignUpValidationStatus.Success;
            }
            catch (DatabaseOperationException)
            {
                status = SignUpValidationStatus.DatabaseError;
            }
        }

        return status;
    }

    public SignUpValidationStatus RegisterUser(
        Player player,
        string password,
        string language)
    {
        SignUpValidationStatus status =
            SignUpValidationStatus.EmptyFields;

        if (player != null)
        {
            status = ValidateInput(
                player.Username,
                player.Email,
                password);

            if (status == SignUpValidationStatus.Success)
            {
                try
                {
                    player.PasswordHash =
                        PasswordHasher.Hash(password);

                    bool isSaved =
                        _store.SaveNewPlayer(player, language);

                    status = isSaved
                        ? SignUpValidationStatus.Success
                        : SignUpValidationStatus.UserAlreadyExists;
                }
                catch (DatabaseOperationException)
                {
                    status = SignUpValidationStatus.DatabaseError;
                }
            }
        }

        return status;
    }
}
