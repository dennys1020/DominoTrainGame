using System;
using log4net.Ext.Trace;

namespace DominoTrainGame.Server.Application.UseCases.Validation;

public static class PlayerInputValidator
{
    public const int MaximumUsernameLength = 45;
    public const int MaximumEmailLength = 255;

    private const int MaximumPasswordLength = 128;

    private static readonly ITraceLog _logger;

    static PlayerInputValidator()
    {
        _logger = TraceLogManager.GetLogger(typeof(PlayerInputValidator));
    }

    public static string NormalizeUsername(string username)
    {
        string normalizedUsername = InputText.Normalize(username);
        bool isUsernameValid = !string.IsNullOrEmpty(normalizedUsername)
            && normalizedUsername.Length <= MaximumUsernameLength;

        if (!isUsernameValid)
        {
            _logger.Warn("A username that is empty or longer than the maximum length was rejected.");
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
            _logger.Warn("A password shorter or longer than the allowed length was rejected.");
            throw new ArgumentException(
                "The password is shorter or longer than the allowed length.",
                nameof(password));
        }
    }
}
