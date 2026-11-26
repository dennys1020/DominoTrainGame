using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Shared.Networking;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class LobbyChannel
{
    private const int MaximumReconnectAttempts = 5;
    private const int ReconnectDelaySeconds = 2;
    private const int NoReconnectAttempts = 0;
    private const string LobbyOperationName = "lobby";

    private static readonly ITraceLog _logger;
    private static readonly TimeSpan _reconnectDelay;
    private static readonly ServiceFailureMapping<LobbyActionResult> _lobbyFailures;

    private readonly IGameConnectionFactory _connectionFactory;
    private readonly ClientSession _session;
    private readonly GameClientCallback _callback;
    private readonly DuplexClientSlot<LobbyServiceClient> _lobbyClientSlot;
    private readonly RemoteCallGuard _callGuard;
    private readonly object _reconnectionSyncRoot = new object();
    private Task _reconnection = Task.CompletedTask;

    static LobbyChannel()
    {
        _logger = TraceLogManager.GetLogger(typeof(LobbyChannel));
        _reconnectDelay = TimeSpan.FromSeconds(ReconnectDelaySeconds);
        _lobbyFailures = new ServiceFailureMapping<LobbyActionResult>(
            LobbyOperationName,
            FaultCodeMapper.MapLobbyFault,
            LobbyActionResult.ServerError,
            LobbyActionResult.ServerUnavailable);
    }

    public LobbyChannel(
        IGameConnectionFactory connectionFactory,
        GameChannelCloser channelCloser,
        ClientSession session,
        GameClientCallback callback)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        _lobbyClientSlot = new DuplexClientSlot<LobbyServiceClient>(channelCloser);
        _callGuard = new RemoteCallGuard();
        _lobbyClientSlot.ClientEnded += HandleLobbyClientEnded;
    }

    public Task OpenAsync(string sessionToken)
    {
        LobbyServiceClient lobbyClient = _connectionFactory.CreateLobbyClient(_callback);
        SessionTokenRequest request = new SessionTokenRequest
        {
            SessionToken = sessionToken
        };
        Task opening = _lobbyClientSlot.OpenAsync(lobbyClient, sessionToken, client => client.ConnectAsync(request));

        return opening;
    }

    public Task<LobbyResult> CreateLobbyAsync()
    {
        Task<LobbyResult> creation = InvokeAsync(
            lobbyClient => StoreLobbyResponseAsync(lobbyClient.CreateLobbyAsync()),
            LobbyResult.CreateFailure);
        return creation;
    }

    public Task<LobbyResult> JoinLobbyAsync(string code)
    {
        Task<LobbyResult> joining = InvokeAsync(
            lobbyClient => RequestJoinLobbyAsync(lobbyClient, code),
            LobbyResult.CreateFailure);
        return joining;
    }

    public Task<LobbyActionResult> SendCommandAsync(Func<LobbyServiceClient, Task> command)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        Task<LobbyActionResult> commandTask = InvokeAsync(
            lobbyClient => RemoteCallGuard.CompleteCommandAsync(command(lobbyClient), LobbyActionResult.Success),
            RemoteCallGuard.KeepStatus<LobbyActionResult>);
        return commandTask;
    }

    public void LogOut()
    {
        _lobbyClientSlot.CloseAfter(SendLogOut);
    }

    private static void SendLogOut(LobbyServiceClient lobbyClient)
    {
        try
        {
            lobbyClient.LogOut();
        }
        catch (TimeoutException exception)
        {
            _logger.Warn("The logout notice did not reach the server before the timeout.", exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn("The logout notice could not reach the server because of a communication error.", exception);
        }
        catch (ObjectDisposedException exception)
        {
            _logger.Warn("The logout notice was not sent because the lobby channel was already closed.", exception);
        }
    }

    private async Task<TResult> InvokeAsync<TResult>(
        Func<LobbyServiceClient, Task<TResult>> call,
        Func<LobbyActionResult, TResult> createFailure)
    {
        Task reconnection;

        lock (_reconnectionSyncRoot)
        {
            reconnection = _reconnection;
        }

        await reconnection.ConfigureAwait(false);
        TResult result = await CallCurrentLobbyClientAsync(call, createFailure).ConfigureAwait(false);
        return result;
    }

    private Task<TResult> CallCurrentLobbyClientAsync<TResult>(
        Func<LobbyServiceClient, Task<TResult>> call,
        Func<LobbyActionResult, TResult> createFailure)
    {
        Task<TResult> callTask = _callGuard.ExecuteAsync(
            () => CallOpenedLobbyClientAsync(call, createFailure),
            _lobbyFailures,
            createFailure);
        return callTask;
    }

    private Task<TResult> CallOpenedLobbyClientAsync<TResult>(
        Func<LobbyServiceClient, Task<TResult>> call,
        Func<LobbyActionResult, TResult> createFailure)
    {
        TResult sessionNotFound = createFailure(LobbyActionResult.SessionNotFound);
        Task<TResult> callTask = _lobbyClientSlot.CallAsync(
            _session.SessionToken,
            call,
            () => Task.FromResult(sessionNotFound));
        return callTask;
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

        if (result.Status == LobbyActionResult.Success)
        {
            result = _session.StoreLobby(result);
        }

        return result;
    }

    private void HandleLobbyClientEnded(object sender, EventArgs eventArgs)
    {
        ReconnectionPlan reconnectionPlan = _session.PrepareReconnection();

        if (reconnectionPlan.CanReconnect)
        {
            _logger.Warn("The lobby channel ended. Trying to reconnect with the session token.");
            ScheduleReconnection(reconnectionPlan.LobbyCode);
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

        lock (_reconnectionSyncRoot)
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
            LobbyActionResult matchResumeResult = await ResumeMatchAsync();
            _logger.InfoFormat("The lobby channel was restored. Attempts: {0}.", attemptCount);
            _callback.ReportConnectionState(ConnectionState.Reconnected);
            ReportMatchResume(matchResumeResult);
        }
        else
        {
            _session.Forget();
            _logger.WarnFormat("The lobby channel could not be restored. Result: {0}.", reconnectResult);
            _callback.ReportConnectionState(ToEndedConnectionState(reconnectResult));
        }
    }

    private static ConnectionState ToEndedConnectionState(ReconnectResult reconnectResult)
    {
        ConnectionState endedState = reconnectResult == ReconnectResult.Rejected
            ? ConnectionState.Expired
            : ConnectionState.Lost;
        return endedState;
    }

    private async Task RejoinLobbyAsync(string lobbyCode)
    {
        bool wasInLobby = !string.IsNullOrEmpty(lobbyCode);

        if (wasInLobby)
        {
            LobbyResult rejoinResult = await CallCurrentLobbyClientAsync(
                lobbyClient => RequestJoinLobbyAsync(lobbyClient, lobbyCode),
                LobbyResult.CreateFailure);
            _logger.InfoFormat("The lobby was rejoined after reconnecting. Status: {0}.", rejoinResult.Status);
        }
    }

    private async Task<LobbyActionResult> ResumeMatchAsync()
    {
        LobbyActionResult resumeResult = LobbyActionResult.Success;
        bool wasInMatch = !string.IsNullOrEmpty(_session.CurrentMatchCode);

        if (wasInMatch)
        {
            resumeResult = await CallCurrentLobbyClientAsync<LobbyActionResult>(
                RequestMatchReconnectionAsync,
                RemoteCallGuard.KeepStatus<LobbyActionResult>).ConfigureAwait(false);
            _logger.InfoFormat("The match reconnection finished. Status: {0}.", resumeResult);
        }

        return resumeResult;
    }

    private Task<LobbyActionResult> RequestMatchReconnectionAsync(LobbyServiceClient lobbyClient)
    {
        ReconnectRequest request = new ReconnectRequest
        {
            MatchCode = _session.CurrentMatchCode
        };
        Task<LobbyActionResult> reconnecting = RemoteCallGuard.CompleteCommandAsync(
            lobbyClient.ReconnectAsync(request),
            LobbyActionResult.Success);

        return reconnecting;
    }

    private void ReportMatchResume(LobbyActionResult resumeResult)
    {
        bool isMatchLost = resumeResult != LobbyActionResult.Success
            && resumeResult != LobbyActionResult.ServerUnavailable;

        if (isMatchLost)
        {
            _logger.WarnFormat("The match seat could not be recovered after reconnecting. Status: {0}.", resumeResult);
            _callback.ReportMatchLost();
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
