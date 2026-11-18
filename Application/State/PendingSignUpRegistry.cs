using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.State;

public sealed class PendingSignUpRegistry
{
    private const int CodeLifetimeMinutes = 10;

    private readonly Dictionary<string, PendingSignUp> _pendingSignUpsByEmail =
        new Dictionary<string, PendingSignUp>(StringComparer.OrdinalIgnoreCase);
    private readonly object _pendingSignUpsLock = new object();
    private readonly IClock _clock;

    public PendingSignUpRegistry(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public void StorePendingSignUp(PlayerRegistration playerRegistration, string verificationCode)
    {
        if (playerRegistration == null)
        {
            throw new ArgumentNullException(nameof(playerRegistration));
        }

        DateTime expiresAt = _clock.UtcNow.AddMinutes(CodeLifetimeMinutes);
        PendingSignUp pendingSignUp = new PendingSignUp(playerRegistration, verificationCode, expiresAt);

        lock (_pendingSignUpsLock)
        {
            _pendingSignUpsByEmail[playerRegistration.Email] = pendingSignUp;
        }
    }

    public VerifyEmailStatus VerifyPendingSignUpCode(string email, string verificationCode)
    {
        DateTime currentTime = _clock.UtcNow;
        VerifyEmailStatus status = VerifyEmailStatus.RegistrationNotFound;

        lock (_pendingSignUpsLock)
        {
            PendingSignUp pendingSignUp = GetPendingSignUpOrEmpty(email);

            if (!pendingSignUp.IsEmpty)
            {
                status = pendingSignUp.VerifyCode(verificationCode, currentTime);
            }
        }

        return status;
    }

    public PendingSignUp FindPendingSignUpByEmail(string email)
    {
        PendingSignUp pendingSignUp;

        lock (_pendingSignUpsLock)
        {
            pendingSignUp = GetPendingSignUpOrEmpty(email);
        }

        return pendingSignUp;
    }

    public void RemovePendingSignUp(string email)
    {
        lock (_pendingSignUpsLock)
        {
            _pendingSignUpsByEmail.Remove(email ?? string.Empty);
        }
    }

    private PendingSignUp GetPendingSignUpOrEmpty(string email)
    {
        string emailKey = email ?? string.Empty;
        PendingSignUp pendingSignUp = _pendingSignUpsByEmail.ContainsKey(emailKey)
            ? _pendingSignUpsByEmail[emailKey]
            : PendingSignUp.Empty;
        return pendingSignUp;
    }
}
