using System;
using DominoTrainGame.Client.Models.DominoTrainService;

namespace DominoTrainGame.Client.Models.Notifications;

public sealed class ServiceOperationEventArgs : EventArgs
{
    public ServiceOperationEventArgs(Guid requestId, OperationState state, string message)
    {
        RequestId = requestId;
        State = state;
        Message = message;
    }
    public Guid RequestId { get; }
    public OperationState State { get; }
    public string Message { get; }
}
