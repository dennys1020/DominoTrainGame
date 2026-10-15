using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Callbacks;

internal sealed class PendingResponse<TResult>
{
    private readonly TaskCompletionSource<TResult> _completion =
        new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

    public Guid RequestId { get; } = Guid.NewGuid();

    public void Complete(Guid requestId, TResult result)
    {
        if (requestId == RequestId) { _completion.TrySetResult(result); }
    }

    public async Task<TResult> InvokeAsync(IClientChannel channel, Func<Task> send,
        CancellationToken cancellationToken = default)
    {
        TResult result;
        channel.Closed += OnConnectionUnavailable;
        channel.Faulted += OnConnectionUnavailable;

        try
        {
            using (Timer timer = new Timer(OnTimeout, null, channel.OperationTimeout, Timeout.InfiniteTimeSpan))
            using (cancellationToken.Register(OnCancelled))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await send().ConfigureAwait(false);
                result = await _completion.Task.ConfigureAwait(false);
            }
        }
        finally
        {
            channel.Closed -= OnConnectionUnavailable;
            channel.Faulted -= OnConnectionUnavailable;
        }

        return result;
    }

    private void OnConnectionUnavailable(object sender, EventArgs arguments)
    {
        _completion.TrySetException(new CommunicationException("The callback channel is unavailable."));
    }

    private void OnTimeout(object state)
    {
        _completion.TrySetException(new TimeoutException("The server callback did not arrive in time."));
    }

    private void OnCancelled()
    {
        _completion.TrySetCanceled();
    }
}
