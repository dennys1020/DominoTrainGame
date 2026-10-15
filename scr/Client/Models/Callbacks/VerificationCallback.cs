using System;
using System.ServiceModel;
using DominoTrainGame.Client.Models.DominoTrainService;
using DominoTrainGame.Client.Models.Errors;

namespace DominoTrainGame.Client.Models.Callbacks;

[CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Single)]
internal sealed class VerificationCallback : OperationCallback<VerificationSessionDto>, IVerificationServiceCallback
{
    public VerificationCallback(Guid requestId, Action<Guid, OperationState, string> report) : base(requestId, report) { }

    public void OnVerificationChanged(VerificationNotification notification)
    {
        if (notification is null) { Fail(new GameServiceException("The server returned an empty verification notification.")); }
        else { Receive(notification.RequestId, notification.State, notification.Message, notification.Result); }
    }
}
