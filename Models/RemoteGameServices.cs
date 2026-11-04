using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteGameServices : IRemoteGameServices
{
    private const int NoHostPlayerId = 0;
    private const int NoPlayerId = 0;
    private const int MaximumReconnectAttempts = 5;
    private const int ReconnectDelaySeconds = 2;
    private const string ConnectionOperationName = "connection";
    private const string LoginOperationName = "login";
    private const string SignUpOperationName = "sign up";

    private static readonly ITraceLog _logger;
    private static readonly TimeSpan _reconnectDelay;
    private static readonly ServiceFailureMapping<ConnectionCheckResult> _connectionFailures;
    private static readonly ServiceFailureMapping<LoginResult> _loginFailures;
    private static readonly ServiceFailureMapping<SignUpResult> _signUpFailures;

    private readonly IGameConnectionFactory _connectionFactory;
    private readonly GameChannelCloser _channelCloser;
    private readonly object _sessionSyncRoot = new object();
    private readonly List<Func<LobbyResult, LobbyResult>> _pendingLobbyUpdates =
        new List<Func<LobbyResult, LobbyResult>>();
    private LobbyServiceClient _lobbyClient;
    private LobbyResult _currentLobby = LobbyResult.Empty;
    private Task _reconnection = Task.CompletedTask;
    private int _currentPlayerId;
    private string _sessionToken = string.Empty;
    private SynchronizationContext _synchronizationContext = new SynchronizationContext();

    static RemoteGameServices()
    {
        _logger = TraceLogManager.GetLogger(typeof(RemoteGameServices));
        _reconnectDelay = TimeSpan.FromSeconds(ReconnectDelaySeconds);
        _connectionFailures = new ServiceFailureMapping<ConnectionCheckResult>(
            MapConnectionFault,
            ConnectionCheckResult.ServerError,
            ConnectionCheckResult.ServerUnavailable);
        _loginFailures = new ServiceFailureMapping<LoginResult>(
            MapLoginFault,
            LoginResult.ServerError,
            LoginResult.ServerUnavailable);
        _signUpFailures = new ServiceFailureMapping<SignUpResult>(
            MapSignUpFault,
            SignUpResult.ServerError,
            SignUpResult.ServerUnavailable);
    }

    public RemoteGameServices(IGameConnectionFactory connectionFactory, GameChannelCloser channelCloser)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _channelCloser = channelCloser ?? throw new ArgumentNullException(nameof(channelCloser));
    }

    public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;

    public event EventHandler<PlayerLeftEventArgs> PlayerLeft;

    public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

    public event EventHandler<PlayerReadyChangedEventArgs> PlayerReadyChanged;

    public event EventHandler<LobbyMessageReceivedEventArgs> LobbyMessageReceived;

    public event EventHandler<MatchStartingEventArgs> MatchStarting;

    public LobbyResult CurrentLobby
    {
        get
        {
            LobbyResult currentLobby;

            lock (_sessionSyncRoot)
            {
                currentLobby = _currentLobby;
            }

            return currentLobby;
        }
    }

    public int CurrentPlayerId
    {
        get
        {
            int currentPlayerId;

            lock (_sessionSyncRoot)
            {
                currentPlayerId = _currentPlayerId;
            }

            return currentPlayerId;
        }
    }

    public string SessionToken
    {
        get
        {
            string sessionToken;

            lock (_sessionSyncRoot)
            {
                sessionToken = _sessionToken;
            }

            return sessionToken;
        }
    }

    public async Task<ConnectionCheckResult> CheckConnectionAsync()
    {
        ConnectionCheckResult connectionResult = await SendServiceRequestAsync(
            SendCheckConnectionAsync,
            _connectionFailures,
            ConnectionOperationName).ConfigureAwait(false);
        return connectionResult;
    }

    public async Task<LoginResult> LogInAsync(string identifier, string password)
    {
        SynchronizationContext callerContext = SynchronizationContext.Current;
        LoginResult loginResult = await SendServiceRequestAsync(
            () => AuthenticateAsync(identifier, password, callerContext),
            _loginFailures,
            LoginOperationName);
        bool isLoggedIn = loginResult == LoginResult.Success;

        if (!isLoggedIn)
        {
            ForgetSession();
        }

        return loginResult;
    }

    public async Task<LobbyResult> CreateLobbyAsync()
    {
        LobbyResult result = await InvokeLobbyServiceAsync(
            lobbyClient => StoreLobbyResponseAsync(lobbyClient.CreateLobbyAsync()),
            CreateEmptyLobbyResult).ConfigureAwait(false);
        return result;
    }

    public async Task<LobbyResult> JoinLobbyAsync(string code)
    {
        LobbyResult result = await InvokeLobbyServiceAsync(
            lobbyClient => RequestJoinLobbyAsync(lobbyClient, code),
            CreateEmptyLobbyResult).ConfigureAwait(false);
        return result;
    }

    public async Task<LobbyResultStatus> LeaveLobbyAsync()
    {
        LobbyResultStatus resultStatus = await InvokeLobbyServiceAsync(
            lobbyClient => CompleteLobbyCommandAsync(lobbyClient.LeaveLobbyAsync()),
            KeepStatus).ConfigureAwait(false);

        ClearCurrentLobby();
        return resultStatus;
    }

    public async Task<LobbyResultStatus> SetReadyAsync(bool isReady)
    {
        SetReadyRequest request = new SetReadyRequest
        {
            IsReady = isReady
        };
        LobbyResultStatus resultStatus = await InvokeLobbyServiceAsync(
            lobbyClient => CompleteLobbyCommandAsync(lobbyClient.SetReadyAsync(request)),
            KeepStatus).ConfigureAwait(false);
        return resultStatus;
    }

    public async Task<LobbyResultStatus> SendLobbyMessageAsync(string text)
    {
        SendLobbyMessageRequest request = new SendLobbyMessageRequest
        {
            Text = text
        };
        LobbyResultStatus resultStatus = await InvokeLobbyServiceAsync(
            lobbyClient => CompleteLobbyCommandAsync(lobbyClient.SendLobbyMessageAsync(request)),
            KeepStatus).ConfigureAwait(false);
        return resultStatus;
    }

    public async Task<LobbyResultStatus> StartMatchAsync()
    {
        LobbyResultStatus resultStatus = await InvokeLobbyServiceAsync(
            lobbyClient => CompleteLobbyCommandAsync(lobbyClient.StartMatchAsync()),
            KeepStatus).ConfigureAwait(false);
        return resultStatus;
    }

    public async Task<SignUpResult> SignUpAsync(string username, string email, string password)
    {
        SignUpResult signUpResult = await SendServiceRequestAsync(
            () => SendSignUpAsync(username, email, password),
            _signUpFailures,
            SignUpOperationName).ConfigureAwait(false);
        return signUpResult;
    }

    private static async Task<TResult> SendServiceRequestAsync<TResult>(
        Func<Task<TResult>> request,
        ServiceFailureMapping<TResult> failures,
        string operationName)
    {
        TResult result;

        try
        {
            result = await request().ConfigureAwait(false);
        }
        catch (FaultException<GameServiceFault> exception)
        {
            _logger.WarnFormat(
                "The server rejected the {0} request. FaultCode: {1}.",
                operationName,
                exception.Detail.Code);
            result = failures.MapFault(exception.Detail.Code);
        }
        catch (FaultException exception)
        {
            _logger.Error("The server reported an undeclared fault while processing a request.", exception);
            result = failures.ServerError;
        }
        catch (TimeoutException exception)
        {
            _logger.Error("A request did not finish before the configured timeout.", exception);
            result = failures.ServerUnavailable;
        }
        catch (CommunicationException exception)
        {
            _logger.Error("A request could not reach the server.", exception);
            result = failures.ServerUnavailable;
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error("The service endpoint is missing or invalid in App.config.", exception);
            result = failures.ServerUnavailable;
        }

        return result;
    }

    private async Task<LoginResult> AuthenticateAsync(
        string identifier,
        string password,
        SynchronizationContext callerContext)
    {
        LoginResult loginResult = LoginResult.InvalidCredentials;
        LoginResponse response = await SendLogInAsync(identifier, password);
        bool isAuthenticated = response?.IsAuthenticated ?? false;

        if (isAuthenticated)
        {
            StoreSession(response, callerContext);
            await OpenLobbyChannelAsync(response.SessionToken);
            loginResult = LoginResult.Success;
        }

        return loginResult;
    }

    private async Task<TResult> InvokeLobbyServiceAsync<TResult>(
        Func<LobbyServiceClient, Task<TResult>> call,
        Func<LobbyResultStatus, TResult> createFailure)
    {
        Task reconnection;

        lock (_sessionSyncRoot)
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

        lock (_sessionSyncRoot)
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
            result = createFailure(MapLobbyFault(exception.Detail.Code));
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
            _logger.Error("The lobby endpoint is missing or invalid in App.config.", exception);
            result = createFailure(LobbyResultStatus.ServerError);
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
        LobbyResult result = MapLobbyResponse(response);

        if (result.Status == LobbyResultStatus.Success)
        {
            result = SetCurrentLobby(result);
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

    private void ClearCurrentLobby()
    {
        lock (_sessionSyncRoot)
        {
            _currentLobby = LobbyResult.Empty;
            _pendingLobbyUpdates.Clear();
        }
    }

    private static ConnectionCheckResult MapConnectionFault(GameServiceFaultCode faultCode)
    {
        ConnectionCheckResult connectionResult;

        switch (faultCode)
        {
            case GameServiceFaultCode.DatabaseUnavailable:
                connectionResult = ConnectionCheckResult.DatabaseUnavailable;
                break;
            default:
                connectionResult = ConnectionCheckResult.ServerError;
                break;
        }

        return connectionResult;
    }

    private static LoginResult MapLoginFault(GameServiceFaultCode faultCode)
    {
        LoginResult loginResult;

        switch (faultCode)
        {
            case GameServiceFaultCode.InvalidInput:
                loginResult = LoginResult.InvalidInput;
                break;
            case GameServiceFaultCode.DatabaseUnavailable:
                loginResult = LoginResult.DatabaseUnavailable;
                break;
            case GameServiceFaultCode.AlreadyConnected:
                loginResult = LoginResult.AlreadyConnected;
                break;
            default:
                loginResult = LoginResult.ServerError;
                break;
        }

        return loginResult;
    }

    private static LobbyResultStatus MapLobbyFault(GameServiceFaultCode faultCode)
    {
        LobbyResultStatus resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.InvalidLobbyRequest:
                resultStatus = LobbyResultStatus.InvalidCode;
                break;
            case GameServiceFaultCode.InvalidInput:
                resultStatus = LobbyResultStatus.InvalidInput;
                break;
            case GameServiceFaultCode.LobbyNotFound:
                resultStatus = LobbyResultStatus.LobbyNotFound;
                break;
            case GameServiceFaultCode.LobbyFull:
                resultStatus = LobbyResultStatus.LobbyFull;
                break;
            case GameServiceFaultCode.LobbyConflict:
                resultStatus = LobbyResultStatus.AlreadyInLobby;
                break;
            case GameServiceFaultCode.NotLobbyHost:
                resultStatus = LobbyResultStatus.NotLobbyHost;
                break;
            case GameServiceFaultCode.PlayersNotReady:
                resultStatus = LobbyResultStatus.PlayersNotReady;
                break;
            default:
                resultStatus = MapSessionFault(faultCode);
                break;
        }

        return resultStatus;
    }

    private static LobbyResultStatus MapSessionFault(GameServiceFaultCode faultCode)
    {
        LobbyResultStatus resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.SessionNotFound:
            case GameServiceFaultCode.InvalidSessionToken:
                resultStatus = LobbyResultStatus.SessionNotFound;
                break;
            case GameServiceFaultCode.AlreadyConnected:
                resultStatus = LobbyResultStatus.AlreadyConnected;
                break;
            default:
                resultStatus = LobbyResultStatus.ServerError;
                break;
        }

        return resultStatus;
    }

    private static LobbyResult CreateEmptyLobbyResult(LobbyResultStatus status)
    {
        LobbyResult result = new LobbyResult(status, LobbySettings.Empty, LobbyRoster.Empty);
        return result;
    }

    private static LobbyResult MapLobbyResponse(LobbyResponse response)
    {
        LobbyResult result = CreateEmptyLobbyResult(LobbyResultStatus.ServerError);

        if (response != null)
        {
            LobbySettings settings = new LobbySettings(response.Code, response.MaximumPlayers);
            result = new LobbyResult(LobbyResultStatus.Success, settings, MapLobbyRoster(response));
        }

        return result;
    }

    private static LobbyRoster MapLobbyRoster(LobbyResponse response)
    {
        List<LobbyPlayerResult> players = new List<LobbyPlayerResult>();
        int hostPlayerId = NoHostPlayerId;

        foreach (LobbyPlayer player in response.Players ?? Array.Empty<LobbyPlayer>())
        {
            if (player != null)
            {
                players.Add(MapLobbyPlayer(player));
                hostPlayerId = player.IsHost ? player.PlayerId : hostPlayerId;
            }
        }

        LobbyRoster roster = new LobbyRoster(response.Version, hostPlayerId, players);
        return roster;
    }

    private static LobbyPlayerResult MapLobbyPlayer(LobbyPlayer player)
    {
        LobbyMember member = new LobbyMember(player.PlayerId, player.Username);
        LobbyPlayerResult lobbyPlayer = new LobbyPlayerResult(member, player.IsHost, player.IsReady);
        return lobbyPlayer;
    }

    private LobbyResult SetCurrentLobby(LobbyResult lobby)
    {
        LobbyResult currentLobby;

        lock (_sessionSyncRoot)
        {
            _currentLobby = lobby;

            foreach (Func<LobbyResult, LobbyResult> pendingUpdate in _pendingLobbyUpdates)
            {
                _currentLobby = pendingUpdate(_currentLobby);
            }

            _pendingLobbyUpdates.Clear();
            currentLobby = _currentLobby;
        }

        return currentLobby;
    }

    private void HandlePlayerJoined(object sender, PlayerJoinedEventArgs e)
    {
        bool wasApplied = TryApplyLobbyUpdate(e.Version, lobby => CreateLobbyWithPlayer(lobby, e));

        if (wasApplied)
        {
            OnPlayerJoined(e);
        }
    }

    private void HandlePlayerLeft(object sender, PlayerLeftEventArgs e)
    {
        bool wasApplied = TryApplyLobbyUpdate(e.Version, lobby => CreateLobbyWithoutPlayer(lobby, e));

        if (wasApplied)
        {
            OnPlayerLeft(e);
        }
    }

    private void HandlePlayerReadyChanged(object sender, PlayerReadyChangedEventArgs e)
    {
        bool wasApplied = TryApplyLobbyUpdate(e.Version, lobby => CreateLobbyWithReadyState(lobby, e));

        if (wasApplied)
        {
            OnPlayerReadyChanged(e);
        }
    }

    private void HandleLobbyMessageReceived(object sender, LobbyMessageReceivedEventArgs e)
    {
        OnLobbyMessageReceived(e);
    }

    private void HandleMatchStarting(object sender, MatchStartingEventArgs e)
    {
        OnMatchStarting(e);
    }

    private bool TryApplyLobbyUpdate(long version, Func<LobbyResult, LobbyResult> update)
    {
        bool wasApplied;

        lock (_sessionSyncRoot)
        {
            bool hasLobby = _currentLobby.Status == LobbyResultStatus.Success;
            wasApplied = hasLobby && version > _currentLobby.Version;

            if (!hasLobby)
            {
                _pendingLobbyUpdates.Add(lobby => ApplyIfNewer(lobby, version, update));
            }
            else if (wasApplied)
            {
                _currentLobby = update(_currentLobby);
            }
        }

        return wasApplied;
    }

    private static LobbyResult ApplyIfNewer(LobbyResult lobby, long version, Func<LobbyResult, LobbyResult> update)
    {
        LobbyResult updatedLobby = lobby;
        bool isNewerVersion = version > lobby.Version;

        if (isNewerVersion)
        {
            updatedLobby = update(lobby);
        }

        return updatedLobby;
    }

    private void OnPlayerJoined(PlayerJoinedEventArgs eventArgs)
    {
        PlayerJoined?.Invoke(this, eventArgs);
    }

    private void OnPlayerLeft(PlayerLeftEventArgs eventArgs)
    {
        PlayerLeft?.Invoke(this, eventArgs);
    }

    private void OnPlayerReadyChanged(PlayerReadyChangedEventArgs eventArgs)
    {
        PlayerReadyChanged?.Invoke(this, eventArgs);
    }

    private void OnLobbyMessageReceived(LobbyMessageReceivedEventArgs eventArgs)
    {
        LobbyMessageReceived?.Invoke(this, eventArgs);
    }

    private void OnMatchStarting(MatchStartingEventArgs eventArgs)
    {
        MatchStarting?.Invoke(this, eventArgs);
    }

    private static LobbyResult CreateLobbyWithPlayer(LobbyResult lobby, PlayerJoinedEventArgs eventArgs)
    {
        List<LobbyPlayerResult> players = new List<LobbyPlayerResult>(lobby.Players);
        bool alreadyPresent = players.Exists(player => player.PlayerId == eventArgs.Player.PlayerId);

        if (!alreadyPresent)
        {
            players.Add(eventArgs.Player);
        }

        LobbyRoster roster = new LobbyRoster(eventArgs.Version, lobby.HostPlayerId, players);
        LobbyResult updatedLobby = lobby.WithRoster(roster);
        return updatedLobby;
    }

    private static LobbyResult CreateLobbyWithoutPlayer(LobbyResult lobby, PlayerLeftEventArgs eventArgs)
    {
        List<LobbyPlayerResult> players = new List<LobbyPlayerResult>();

        foreach (LobbyPlayerResult player in lobby.Players)
        {
            if (player.PlayerId != eventArgs.PlayerId)
            {
                bool isHost = player.PlayerId == eventArgs.HostPlayerId;
                players.Add(player.WithHostState(isHost));
            }
        }

        LobbyRoster roster = new LobbyRoster(eventArgs.Version, eventArgs.HostPlayerId, players);
        LobbyResult updatedLobby = lobby.WithRoster(roster);
        return updatedLobby;
    }

    private static LobbyResult CreateLobbyWithReadyState(LobbyResult lobby, PlayerReadyChangedEventArgs eventArgs)
    {
        List<LobbyPlayerResult> players = new List<LobbyPlayerResult>();

        foreach (LobbyPlayerResult player in lobby.Players)
        {
            LobbyPlayerResult updatedPlayer = player.PlayerId == eventArgs.PlayerId
                ? player.WithReadyState(eventArgs.IsReady)
                : player;

            players.Add(updatedPlayer);
        }

        LobbyRoster roster = new LobbyRoster(eventArgs.Version, lobby.HostPlayerId, players);
        LobbyResult updatedLobby = lobby.WithRoster(roster);
        return updatedLobby;
    }

    private GameClientCallback CreateCallback()
    {
        GameClientCallback callback = new GameClientCallback();
        callback.PlayerJoined += HandlePlayerJoined;
        callback.PlayerLeft += HandlePlayerLeft;
        callback.PlayerReadyChanged += HandlePlayerReadyChanged;
        callback.LobbyMessageReceived += HandleLobbyMessageReceived;
        callback.MatchStarting += HandleMatchStarting;
        return callback;
    }

    private void StoreSession(LoginResponse response, SynchronizationContext callerContext)
    {
        lock (_sessionSyncRoot)
        {
            _synchronizationContext = callerContext ?? _synchronizationContext;
            _currentPlayerId = response.PlayerId;
            _sessionToken = response.SessionToken ?? string.Empty;
        }
    }

    private async Task OpenLobbyChannelAsync(string sessionToken)
    {
        LobbyServiceClient lobbyClient = _connectionFactory.CreateLobbyClient(CreateCallback());
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

    private void AttachLobbyClient(LobbyServiceClient lobbyClient)
    {
        ICommunicationObject channel = lobbyClient;

        channel.Faulted += (sender, e) => RecoverLobbyChannel(lobbyClient);
        channel.Closed += (sender, e) => RecoverLobbyChannel(lobbyClient);

        lock (_sessionSyncRoot)
        {
            _lobbyClient = lobbyClient;
        }
    }

    private void RecoverLobbyChannel(LobbyServiceClient endedClient)
    {
        bool isCurrentChannel;
        bool canReconnect = false;
        string lobbyCode = string.Empty;

        lock (_sessionSyncRoot)
        {
            isCurrentChannel = ReferenceEquals(endedClient, _lobbyClient);

            if (isCurrentChannel)
            {
                canReconnect = !string.IsNullOrEmpty(_sessionToken);
                lobbyCode = _currentLobby.Code;

                _lobbyClient = null;
                _currentLobby = LobbyResult.Empty;
                _pendingLobbyUpdates.Clear();

                if (!canReconnect)
                {
                    _currentPlayerId = NoPlayerId;
                }
            }
        }

        if (isCurrentChannel && canReconnect)
        {
            _logger.Warn("The lobby channel ended. Trying to reconnect with the session token.");
            ScheduleReconnection(lobbyCode);
        }
        else if (isCurrentChannel)
        {
            _logger.Warn("The lobby channel ended and there is no session token to reconnect.");
        }
    }

    private void ScheduleReconnection(string lobbyCode)
    {
        SynchronizationContext context;

        lock (_sessionSyncRoot)
        {
            context = _synchronizationContext;
        }

        context.Post(StartReconnection, lobbyCode);
    }

    private void StartReconnection(object state)
    {
        string lobbyCode = state?.ToString() ?? string.Empty;
        Task reconnection = ReconnectAsync(lobbyCode);

        lock (_sessionSyncRoot)
        {
            _reconnection = reconnection;
        }
    }

    private async Task ReconnectAsync(string lobbyCode)
    {
        OnConnectionStateChanged(new ConnectionStateChangedEventArgs(ConnectionState.Reconnecting));

        ReconnectResult reconnectResult = ReconnectResult.Unavailable;
        int attemptCount = 0;

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
            OnConnectionStateChanged(new ConnectionStateChangedEventArgs(ConnectionState.Reconnected));
        }
        else
        {
            ForgetSession();
            _logger.WarnFormat("The lobby channel could not be restored. Result: {0}.", reconnectResult);
            OnConnectionStateChanged(new ConnectionStateChangedEventArgs(ConnectionState.Lost));
        }
    }

    private async Task RejoinLobbyAsync(string lobbyCode)
    {
        bool wasInLobby = !string.IsNullOrEmpty(lobbyCode);

        if (wasInLobby)
        {
            LobbyResult rejoinResult = await CallCurrentLobbyClientAsync(
                lobbyClient => RequestJoinLobbyAsync(lobbyClient, lobbyCode),
                CreateEmptyLobbyResult);
            _logger.InfoFormat("The lobby was rejoined after reconnecting. Status: {0}.", rejoinResult.Status);
        }
    }

    private void ForgetSession()
    {
        lock (_sessionSyncRoot)
        {
            _sessionToken = string.Empty;
            _currentPlayerId = NoPlayerId;
        }
    }

    private async Task<ReconnectResult> TryReconnectOnceAsync()
    {
        ReconnectResult reconnectResult = ReconnectResult.Unavailable;
        string sessionToken;

        lock (_sessionSyncRoot)
        {
            sessionToken = _sessionToken;
        }

        try
        {
            await OpenLobbyChannelAsync(sessionToken).ConfigureAwait(false);
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

    private void OnConnectionStateChanged(ConnectionStateChangedEventArgs eventArgs)
    {
        ConnectionStateChanged?.Invoke(this, eventArgs);
    }

    private static SignUpResult MapSignUpFault(GameServiceFaultCode faultCode)
    {
        SignUpResult signUpResult;

        switch (faultCode)
        {
            case GameServiceFaultCode.InvalidInput:
                signUpResult = SignUpResult.InvalidInput;
                break;
            case GameServiceFaultCode.DatabaseUnavailable:
                signUpResult = SignUpResult.DatabaseUnavailable;
                break;
            default:
                signUpResult = SignUpResult.ServerError;
                break;
        }

        return signUpResult;
    }

    private async Task<ConnectionCheckResult> SendCheckConnectionAsync()
    {
        ConnectionServiceClient connectionClient = _connectionFactory.CreateConnectionClient();
        ConnectionCheckResult connectionResult;

        try
        {
            await connectionClient.CheckConnectionAsync().ConfigureAwait(false);
            connectionResult = ConnectionCheckResult.Connected;
        }
        finally
        {
            _channelCloser.Close(connectionClient);
        }

        return connectionResult;
    }

    private async Task<LoginResponse> SendLogInAsync(string identifier, string password)
    {
        AuthenticationServiceClient authenticationClient = _connectionFactory.CreateAuthenticationClient();
        LoginResponse response;

        try
        {
            LoginRequest request = new LoginRequest
            {
                Identifier = identifier,
                Password = password
            };

            response = await authenticationClient.LogInAsync(request).ConfigureAwait(false);
        }
        finally
        {
            _channelCloser.Close(authenticationClient);
        }

        return response;
    }

    private async Task<SignUpResult> SendSignUpAsync(string username, string email, string password)
    {
        AuthenticationServiceClient authenticationClient = _connectionFactory.CreateAuthenticationClient();
        SignUpResult signUpResult;

        try
        {
            SignUpRequest request = new SignUpRequest
            {
                Username = username,
                Email = email,
                Password = password
            };

            bool isRegistered = await authenticationClient.SignUpAsync(request).ConfigureAwait(false);
            signUpResult = isRegistered
                ? SignUpResult.Success
                : SignUpResult.AlreadyExists;
        }
        finally
        {
            _channelCloser.Close(authenticationClient);
        }

        return signUpResult;
    }

    private enum ReconnectResult
    {
        Connected,
        Rejected,
        Unavailable
    }
}
