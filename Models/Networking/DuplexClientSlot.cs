using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Networking;

internal sealed class DuplexClientSlot<TClient>
    where TClient : class, ICommunicationObject
{
    private readonly GameChannelCloser _channelCloser;
    private readonly object _clientSyncRoot = new object();
    private TClient _client;
    private string _sessionToken = string.Empty;

    public DuplexClientSlot(GameChannelCloser channelCloser)
    {
        _channelCloser = channelCloser ?? throw new ArgumentNullException(nameof(channelCloser));
    }

    public event EventHandler<EventArgs> ClientEnded;

    public Task OpenAsync(TClient client, string sessionToken, Func<TClient, Task> connect)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (connect == null)
        {
            throw new ArgumentNullException(nameof(connect));
        }

        Task opening = OpenCoreAsync(client, sessionToken ?? string.Empty, connect);
        return opening;
    }

    public Task<TResult> CallAsync<TResult>(
        string sessionToken,
        Func<TClient, Task<TResult>> call,
        Func<Task<TResult>> callWhenClosed)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        if (callWhenClosed == null)
        {
            throw new ArgumentNullException(nameof(callWhenClosed));
        }

        TClient client;
        bool isReusable;

        lock (_clientSyncRoot)
        {
            client = _client;
            isReusable = client?.State == CommunicationState.Opened
                && string.Equals(_sessionToken, sessionToken, StringComparison.Ordinal);
        }

        Task<TResult> callTask = isReusable ? call(client) : callWhenClosed();
        return callTask;
    }

    public void Close()
    {
        TClient closedClient;

        lock (_clientSyncRoot)
        {
            closedClient = _client;
            _client = null;
            _sessionToken = string.Empty;
        }

        if (closedClient != null)
        {
            _channelCloser.Close(closedClient);
        }
    }

    private async Task OpenCoreAsync(TClient client, string sessionToken, Func<TClient, Task> connect)
    {
        bool isConnected = false;

        try
        {
            await connect(client).ConfigureAwait(false);
            Attach(client, sessionToken);
            isConnected = true;
        }
        finally
        {
            if (!isConnected)
            {
                _channelCloser.Close(client);
            }
        }
    }

    private void Attach(TClient client, string sessionToken)
    {
        TClient previousClient;

        client.Faulted += (sender, eventArgs) => ReleaseEndedClient(client);
        client.Closed += (sender, eventArgs) => ReleaseEndedClient(client);

        lock (_clientSyncRoot)
        {
            previousClient = _client;
            _client = client;
            _sessionToken = sessionToken;
        }

        if (previousClient != null)
        {
            _channelCloser.Close(previousClient);
        }
    }

    private void ReleaseEndedClient(TClient endedClient)
    {
        bool isCurrentClient;

        lock (_clientSyncRoot)
        {
            isCurrentClient = ReferenceEquals(endedClient, _client);

            if (isCurrentClient)
            {
                _client = null;
                _sessionToken = string.Empty;
            }
        }

        if (isCurrentClient)
        {
            _channelCloser.Close(endedClient);
            OnClientEnded(EventArgs.Empty);
        }
    }

    private void OnClientEnded(EventArgs eventArgs)
    {
        ClientEnded?.Invoke(this, eventArgs);
    }
}
