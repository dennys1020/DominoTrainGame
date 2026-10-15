using System;
using System.ServiceModel;
using DominoTrainGame.Client.Models.DominoTrainService;

namespace DominoTrainGame.Client.Models.Callbacks;

[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
internal sealed class VerificationCallback : IVerificationServiceCallback
{
    public PendingResponse<VerificationSessionDto> Started { get; } = new PendingResponse<VerificationSessionDto>();
    public PendingResponse<bool> Resent { get; } = new PendingResponse<bool>();
    public PendingResponse<VerificationStatus> Verified { get; } = new PendingResponse<VerificationStatus>();

    public void OnVerificationStarted(Guid requestId, VerificationSessionDto session)
    {
        Started.Complete(requestId, session);
    }

    public void OnVerificationResent(Guid requestId, bool wasSent)
    {
        Resent.Complete(requestId, wasSent);
    }

    public void OnCodeVerified(Guid requestId, VerificationStatus status)
    {
        Verified.Complete(requestId, status);
    }
}
