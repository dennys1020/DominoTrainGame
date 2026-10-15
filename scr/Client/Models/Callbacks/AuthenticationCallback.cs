using System;
using System.ServiceModel;
using DominoTrainGame.Client.Models.DominoTrainService;

namespace DominoTrainGame.Client.Models.Callbacks;

[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
internal sealed class AuthenticationCallback : IAuthenticationServiceCallback
{
    public PendingResponse<LoginStatus> Login { get; } = new PendingResponse<LoginStatus>();
    public PendingResponse<RegistrationStatus> Registration { get; } = new PendingResponse<RegistrationStatus>();
    public PendingResponse<RegistrationStatus> Availability { get; } = new PendingResponse<RegistrationStatus>();

    public void OnLogInCompleted(Guid requestId, LoginStatus status)
    {
        Login.Complete(requestId, status);
    }

    public void OnRegistrationCompleted(Guid requestId, RegistrationStatus status)
    {
        Registration.Complete(requestId, status);
    }

    public void OnAvailabilityChecked(Guid requestId, RegistrationStatus status)
    {
        Availability.Complete(requestId, status);
    }
}
