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

    public void Store(PlayerRegistration playerRegistration, string verificationCode)
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

    public VerifyEmailStatus Verify(string email, string verificationCode)
    {
        DateTime currentTime = _clock.UtcNow;
        VerifyEmailStatus status = VerifyEmailStatus.RegistrationNotFound;

        lock (_pendingSignUpsLock)
        {
            bool isFound = _pendingSignUpsByEmail.TryGetValue(
                email ?? string.Empty,
                out PendingSignUp foundSignUp);

            if (isFound)
            {
                status = foundSignUp.Verify(verificationCode, currentTime);
            }
        }

        return status;
    }

    public bool TryFind(string email, out PendingSignUp pendingSignUp)
    {
        bool isFound;

        lock (_pendingSignUpsLock)
        {
            isFound = _pendingSignUpsByEmail.TryGetValue(email ?? string.Empty, out PendingSignUp foundSignUp);
            pendingSignUp = isFound ? foundSignUp : PendingSignUp.Empty;
        }

        return isFound;
    }

    public void Remove(string email)
    {
        lock (_pendingSignUpsLock)
        {
            _pendingSignUpsByEmail.Remove(email ?? string.Empty);
        }
    }
}
