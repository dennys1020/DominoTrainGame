using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.DominoTrainService;
using DominoTrainGame.Client.Models.Errors;

namespace DominoTrainGame.Client.Models.Callbacks;

internal abstract class OperationCallback<TResult>
{
    private readonly Guid _requestId;
    private readonly Action<Guid, OperationState, string> _report;
    private readonly TaskCompletionSource<TResult> _completion =
        new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

    protected OperationCallback(Guid requestId, Action<Guid, OperationState, string> report)
    {
        _requestId = requestId;
        _report = report;
    }

    public Task<TResult> Completion => _completion.Task;
    public void Fail(Exception exception) => _completion.TrySetException(exception);
    public void Cancel() => _completion.TrySetCanceled();

    protected void Receive(Guid requestId, OperationState state, string message, TResult result)
    {
        if (requestId != _requestId || _completion.Task.IsCompleted) { return; }
        _report(requestId, state, message);
        if (state == OperationState.Completed) { _completion.TrySetResult(result); }
        else if (state == OperationState.Failed) { Fail(new GameServiceException(message)); }
    }
}
