using System;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.State;

public sealed class PendingSignUp
{
    private const int MaximumFailedAttempts = 5;
    private const int CodeLifetimeMinutes = 10;
    private const int ResendCooldownSeconds = 60;

    private readonly string _verificationCode;
    private readonly DateTime _expiresAt;
    private readonly DateTime _resendAvailableAt;

    private int _failedAttempts;

    static PendingSignUp()
    {
        Empty = new PendingSignUp(
            new PlayerRegistration(string.Empty, string.Empty, string.Empty),
            string.Empty,
            DateTime.MinValue);
    }

    public PendingSignUp(PlayerRegistration playerRegistration, string verificationCode, DateTime sentAt)
    {
        Registration = playerRegistration ?? throw new ArgumentNullException(nameof(playerRegistration));
        _verificationCode = verificationCode ?? string.Empty;
        _expiresAt = sentAt.AddMinutes(CodeLifetimeMinutes);
        _resendAvailableAt = sentAt.AddSeconds(ResendCooldownSeconds);
    }

    public static PendingSignUp Empty { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);

    public PlayerRegistration Registration { get; }

    public bool CanResend(DateTime currentTime)
    {
        bool canResend = currentTime >= _resendAvailableAt;
        return canResend;
    }

    public VerifyEmailStatus VerifyCode(string verificationCode, DateTime currentTime)
    {
        VerifyEmailStatus emailStatus = VerifyEmailStatus.Success;
        bool hasTooManyAttempts = _failedAttempts >= MaximumFailedAttempts;
        bool isExpired = currentTime >= _expiresAt;
        bool isCodeCorrect = string.Equals(verificationCode, _verificationCode, StringComparison.Ordinal);

        if (hasTooManyAttempts)
        {
            emailStatus = VerifyEmailStatus.TooManyAttempts;
        }
        else if (isExpired)
        {
            emailStatus = VerifyEmailStatus.CodeExpired;
        }
        else if (!isCodeCorrect)
        {
            _failedAttempts++;
            emailStatus = VerifyEmailStatus.InvalidCode;
        }

        return emailStatus;
    }
}
