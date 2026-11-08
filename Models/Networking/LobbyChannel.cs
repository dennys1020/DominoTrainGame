using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class LobbyChannel
{
    private const int MaximumReconnectAttempts = 5;
    private const int ReconnectDelaySeconds = 2;
    private const int NoReconnectAttempts = 0;

    private static readonly ITraceLog _logger;
    private static readonly TimeSpan _reconnectDelay;

    private readonly IGameConnectionFactory _connectionFactory;
    private readonly GameChannelCloser _channelCloser;
    private readonly ClientSession _session;
    private readonly GameClientCallback _callback;
    private readonly object _channelSyncRoot = new object();
    private LobbyServiceClient _lobbyClient;
    private Task _reconnection = Task.CompletedTask;

    static LobbyChannel()
    {
        _logger = TraceLogManager.GetLogger(typeof(LobbyChannel));
        _reconnectDelay = TimeSpan.FromSeconds(ReconnectDelaySeconds);
    }

    public LobbyChannel(
        IGameConnectionFactory connectionFactory,
        GameChannelCloser channelCloser,
        LobbySessionContext sessionContext)
    {
        if (sessionContext == null)
        {
            throw new ArgumentNullException(nameof(sessionContext));
        }

        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _channelCloser = channelCloser ?? throw new ArgumentNullException(nameof(channelCloser));
        _session = sessionContext.Session;
        _callback = sessionContext.Callback;
    }

    public async Task OpenAsync(string sessionToken)
    {
        LobbyServiceClient lobbyClient = _connectionFactory.CreateLobbyClient(_callback);
        bool isConnected = false;

        try
        {
            ConnectRequest request = new ConnectRequest
            {
                SessionToken = sessionToken
            };

            await lobbyClient.ConnectAsync(request).ConfigureAwait(false);
            AttachLobbyClient(lobbyClient);
            isConnected = true;
        }
        finally
        {
            if (!isConnected)
            {
                _channelCloser.Close(lobbyClient);
            }
        }
    }

    public async Task<LobbyResult> CreateLobbyAsync()
    {
        LobbyResult result = await InvokeAsync(
            lobbyClient => StoreLobbyResponseAsync(lobbyClient.CreateLobbyAsync()),
            LobbyResultMapper.CreateEmptyLobbyResult).ConfigureAwait(false);
        return result;
    }

    public async Task<LobbyResult> JoinLobbyAsync(string code)
    {
        LobbyResult result = await InvokeAsync(
            lobbyClient => RequestJoinLobbyAsync(lobbyClient, code),
            LobbyResultMapper.CreateEmptyLobbyResult).ConfigureAwait(false);
        return result;
    }

    public Task<LobbyResultStatus> SendCommandAsync(Func<LobbyServiceClient, Task> command)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        Task<LobbyResultStatus> commandTask = InvokeAsync(
            lobbyClient => CompleteLobbyCommandAsync(command(lobbyClient)),
            KeepStatus);
        return commandTask;
    }

    public void Close()
    {
        LobbyServiceClient closedClient;

        lock (_channelSyncRoot)
        {
            closedClient = _lobbyClient;
            _lobbyClient = null;
        }

        if (closedClient != null)
        {
            _channelCloser.Close(closedClient);
        }
    }

    private static async Task<TResult> CallLobbyServiceAsync<TResult>(
        LobbyServiceClient lobbyClient,
        Func<LobbyServiceClient, Task<TResult>> call,
        Func<LobbyResultStatus, TResult> createFailure)
    {
        TResult result;

        try
        {
            result = await call(lobbyClient).ConfigureAwait(false);
        }
        catch (FaultException<GameServiceFault> exception)
        {
            _logger.WarnFormat("The server rejected a lobby request. FaultCode: {0}.", exception.Detail.Code);
            result = createFailure(FaultCodeMapper.MapLobbyFault(exception.Detail.Code));
        }
        catch (FaultException exception)
        {
            _logger.Error("The server reported an undeclared fault while processing a lobby request.", exception);
            result = createFailure(LobbyResultStatus.ServerError);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn("A lobby request did not finish before the configured timeout.", exception);
            result = createFailure(LobbyResultStatus.ServerUnavailable);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn("A lobby request failed because of a communication error.", exception);
            result = createFailure(LobbyResultStatus.ServerUnavailable);
        }
        catch (ObjectDisposedException exception)
        {
            _logger.Warn("A lobby request used a lobby channel that was already closed.", exception);
            result = createFailure(LobbyResultStatus.ServerUnavailable);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error(
                "A lobby request could not be sent because the endpoint is invalid or the channel rejected the call.",
                exception);
            result = createFailure(LobbyResultStatus.ServerError);
        }

        return result;
    }

    private static async Task<LobbyResultStatus> CompleteLobbyCommandAsync(Task command)
    {
        await command.ConfigureAwait(false);
        LobbyResultStatus resultStatus = LobbyResultStatus.Success;
        return resultStatus;
    }

    private static LobbyResultStatus KeepStatus(LobbyResultStatus resultStatus)
    {
        return resultStatus;
    }

    private async Task<TResult> InvokeAsync<TResult>(
        Func<LobbyServiceClient, Task<TResult>> call,
        Func<LobbyResultStatus, TResult> createFailure)
    {
        Task reconnection;

        lock (_channelSyncRoot)
        {
            reconnection = _reconnection;
        }

        await reconnection.ConfigureAwait(false);
        TResult result = await CallCurrentLobbyClientAsync(call, createFailure).ConfigureAwait(false);
        return result;
    }

    private async Task<TResult> CallCurrentLobbyClientAsync<TResult>(
        Func<LobbyServiceClient, Task<TResult>> call,
        Func<LobbyResultStatus, TResult> createFailure)
    {
        TResult result = createFailure(LobbyResultStatus.SessionNotFound);
        LobbyServiceClient lobbyClient;

        lock (_channelSyncRoot)
        {
            lobbyClient = _lobbyClient;
        }

        bool hasSession = lobbyClient?.State == CommunicationState.Opened;

        if (hasSession)
        {
            result = await CallLobbyServiceAsync(lobbyClient, call, createFailure).ConfigureAwait(false);
        }

        return result;
    }

    private Task<LobbyResult> RequestJoinLobbyAsync(LobbyServiceClient lobbyClient, string code)
    {
        JoinLobbyRequest request = new JoinLobbyRequest
        {
            Code = code
        };
        Task<LobbyResult> joinTask = StoreLobbyResponseAsync(lobbyClient.JoinLobbyAsync(request));
        return joinTask;
    }

    private async Task<LobbyResult> StoreLobbyResponseAsync(Task<LobbyResponse> responseTask)
    {
        LobbyResponse response = await responseTask.ConfigureAwait(false);
        LobbyResult result = LobbyResultMapper.MapLobbyResponse(response);

        if (result.Status == LobbyResultStatus.Success)
        {
            result = _session.StoreLobby(result);
        }

        return result;
    }

    private void AttachLobbyClient(LobbyServiceClient lobbyClient)
    {
        ICommunicationObject channel = lobbyClient;
        LobbyServiceClient previousClient;

        channel.Faulted += (sender, eventArgs) => RecoverLobbyChannel(lobbyClient);
        channel.Closed += (sender, eventArgs) => RecoverLobbyChannel(lobbyClient);

        lock (_channelSyncRoot)
        {
            previousClient = _lobbyClient;
            _lobbyClient = lobbyClient;
        }

        if (previousClient != null)
        {
            _channelCloser.Close(previousClient);
        }
    }

    private void RecoverLobbyChannel(LobbyServiceClient endedClient)
    {
        bool isCurrentChannel;

        lock (_channelSyncRoot)
        {
            isCurrentChannel = ReferenceEquals(endedClient, _lobbyClient);

            if (isCurrentChannel)
            {
                _lobbyClient = null;
            }
        }

        if (isCurrentChannel)
        {
            HandleEndedChannel();
        }
    }

    private void HandleEndedChannel()
    {
        bool canReconnect = _session.TryPrepareReconnection(out string lobbyCode);

        if (canReconnect)
        {
            _logger.Warn("The lobby channel ended. Trying to reconnect with the session token.");
            ScheduleReconnection(lobbyCode);
        }
        else
        {
            _logger.Warn("The lobby channel ended and there is no session token to reconnect.");
        }
    }

    private void ScheduleReconnection(string lobbyCode)
    {
        SynchronizationContext callerContext = _session.CallerContext;
        callerContext.Post(StartReconnection, lobbyCode);
    }

    private void StartReconnection(object state)
    {
        string lobbyCode = state?.ToString() ?? string.Empty;
        Task reconnection = ReconnectAsync(lobbyCode);

        lock (_channelSyncRoot)
        {
            _reconnection = reconnection;
        }
    }

    private async Task ReconnectAsync(string lobbyCode)
    {
        _callback.ReportConnectionState(ConnectionState.Reconnecting);

        ReconnectResult reconnectResult = ReconnectResult.Unavailable;
        int attemptCount = NoReconnectAttempts;

        while (reconnectResult == ReconnectResult.Unavailable && attemptCount < MaximumReconnectAttempts)
        {
            attemptCount++;
            await Task.Delay(_reconnectDelay);
            reconnectResult = await TryReconnectOnceAsync();
        }

        if (reconnectResult == ReconnectResult.Connected)
        {
            await RejoinLobbyAsync(lobbyCode);
            _logger.InfoFormat("The lobby channel was restored. Attempts: {0}.", attemptCount);
            _callback.ReportConnectionState(ConnectionState.Reconnected);
        }
        else
        {
            _session.Forget();
            _logger.WarnFormat("The lobby channel could not be restored. Result: {0}.", reconnectResult);
            _callback.ReportConnectionState(ConnectionState.Lost);
        }
    }

    private async Task RejoinLobbyAsync(string lobbyCode)
    {
        bool wasInLobby = !string.IsNullOrEmpty(lobbyCode);

        if (wasInLobby)
        {
            LobbyResult rejoinResult = await CallCurrentLobbyClientAsync(
                lobbyClient => RequestJoinLobbyAsync(lobbyClient, lobbyCode),
                LobbyResultMapper.CreateEmptyLobbyResult);
            _logger.InfoFormat("The lobby was rejoined after reconnecting. Status: {0}.", rejoinResult.Status);
        }
    }

    private async Task<ReconnectResult> TryReconnectOnceAsync()
    {
        ReconnectResult reconnectResult = ReconnectResult.Unavailable;
        string sessionToken = _session.SessionToken;

        try
        {
            await OpenAsync(sessionToken).ConfigureAwait(false);
            reconnectResult = ReconnectResult.Connected;
        }
        catch (FaultException<GameServiceFault> exception)
        {
            _logger.WarnFormat("The server rejected the reconnection. FaultCode: {0}.", exception.Detail.Code);

            bool isTokenRejected = exception.Detail.Code == GameServiceFaultCode.InvalidSessionToken;
            reconnectResult = isTokenRejected ? ReconnectResult.Rejected : ReconnectResult.Unavailable;
        }
        catch (FaultException exception)
        {
            _logger.Error("The server reported an undeclared fault while reconnecting.", exception);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn("A reconnection attempt did not finish before the configured timeout.", exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn("A reconnection attempt could not reach the server.", exception);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error("The lobby endpoint is missing or invalid in App.config.", exception);
            reconnectResult = ReconnectResult.Rejected;
        }

        return reconnectResult;
    }

    private enum ReconnectResult
    {
        Connected,
        Rejected,
        Unavailable
    }
}
