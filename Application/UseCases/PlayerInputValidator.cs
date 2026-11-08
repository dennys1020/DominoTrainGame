using System;

namespace DominoTrainGame.Server.Application.UseCases;

public static class PlayerInputValidator
{
    private const int MaximumUsernameLength = 45;
    private const int MaximumPasswordLength = 128;

    public static string NormalizeUsername(string username)
    {
        string normalizedUsername = username?.Trim() ?? string.Empty;
        bool isUsernameValid = !string.IsNullOrEmpty(normalizedUsername)
            && normalizedUsername.Length <= MaximumUsernameLength;

        if (!isUsernameValid)
        {
            throw new ArgumentException(
                "The username is empty or exceeds the maximum allowed length.",
                nameof(username));
        }

        return normalizedUsername;
    }

    public static void ValidatePassword(string password, int minimumLength)
    {
        string enteredPassword = password ?? string.Empty;
        bool isLongEnough = enteredPassword.Length >= minimumLength;
        bool isShortEnough = enteredPassword.Length <= MaximumPasswordLength;

        if (!isLongEnough || !isShortEnough)
        {
            throw new ArgumentException(
                "The password is shorter or longer than the allowed length.",
                nameof(password));
        }
    }
}
