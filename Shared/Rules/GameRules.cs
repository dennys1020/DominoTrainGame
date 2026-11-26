using System;

namespace DominoTrainGame.Shared.Rules;

public static class GameRules
{
    public const int MaximumUsernameLength = 45;
    public const int MaximumEmailLength = 255;
    public const int MinimumPasswordLength = 8;
    public const int MaximumPasswordLength = 128;
    public const int MaximumMessageLength = 200;
    public const int VerificationCodeLength = 6;
    public const int ResendCooldownSeconds = 60;

    public static bool IsUsernameTooLong(string username)
    {
        bool isTooLong = username.Length > MaximumUsernameLength;
        return isTooLong;
    }

    public static bool IsEmailTooLong(string email)
    {
        bool isTooLong = email.Length > MaximumEmailLength;
        return isTooLong;
    }

    public static bool IsPasswordTooShort(string password)
    {
        bool isTooShort = password.Length < MinimumPasswordLength;
        return isTooShort;
    }

    public static bool IsPasswordTooLong(string password)
    {
        bool isTooLong = password.Length > MaximumPasswordLength;
        return isTooLong;
    }

    public static bool IsMessageTooLong(string message)
    {
        bool isTooLong = message.Length > MaximumMessageLength;
        return isTooLong;
    }

    public static bool IsVerificationCodeComplete(string verificationCode)
    {
        bool isComplete = verificationCode.Length == VerificationCodeLength;
        return isComplete;
    }

    public static DateTime CalculateResendAvailableAt(DateTime sentAt)
    {
        DateTime resendAvailableAt = sentAt.AddSeconds(ResendCooldownSeconds);
        return resendAvailableAt;
    }
}
