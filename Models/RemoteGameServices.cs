using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    private static readonly ITraceLog _logger;
    private static readonly int _processId;
    private static readonly TimeSpan _reconnectDelay;

    private readonly IGameConnectionFactory _connectionFactory;
    private readonly GameChannelCloser _channelCloser;
    private readonly object _sessionSyncRoot = new object();
    private readonly List<Func<LobbyResult, LobbyResult>> _pendingLobbyUpdates =
        new List<Func<LobbyResult, LobbyResult>>();
    private LobbyServiceClient _lobbyClient;
    private LobbyResult _currentLobby;
    private int _currentPlayerId;
    private string _sessionToken = string.Empty;
    private SynchronizationContext _synchronizationContext;

    public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;

    public event EventHandler<PlayerLeftEventArgs> PlayerLeft;

    public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

    static RemoteGameServices()
    {
        _logger = TraceLogManager.GetLogger(typeof(RemoteGameServices));
        _processId = Process.GetCurrentProcess().Id;
        _reconnectDelay = TimeSpan.FromSeconds(ReconnectDelaySeconds);
    }

    public RemoteGameServices(IGameConnectionFactory connectionFactory, GameChannelCloser channelCloser)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _channelCloser = channelCloser ?? throw new ArgumentNullException(nameof(channelCloser));
    }

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

    public async Task<ConnectionCheckResult> CheckConnectionAsync()
    {
        ConnectionCheckResult connectionResult = ConnectionCheckResult.ServerUnavailable;

        try
        {
            connectionResult = await SendCheckConnectionAsync().ConfigureAwait(false);
        }
        catch (FaultException<GameServiceFault> exception)
        {
            _logger.WarnFormat("The server rejected the connection request. FaultCode: {0}.", exception.Detail.Code);
            connectionResult = MapConnectionFault(exception.Detail.Code);
        }
        catch (FaultException exception)
        {
            _logger.Error("The server reported an undeclared fault while checking the connection.", exception);
            connectionResult = ConnectionCheckResult.ServerError;
        }
        catch (TimeoutException exception)
        {
            _logger.Error(
                "The server did not answer the connection request before the configured timeout.",
                exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Error("The connection request could not reach the server.", exception);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error("The connection endpoint is missing or invalid in App.config.", exception);
        }

        return connectionResult;
    }

    public async Task<LoginResult> LogInAsync(string identifier, string password)
    {
        LoginResult loginResult = LoginResult.ServerUnavailable;
        SynchronizationContext callerContext = SynchronizationContext.Current;

        try
        {
            // The await keeps the caller context so the duplex channel is opened from the UI thread.
            LoginResponse response = await SendLogInAsync(identifier, password);
            bool isAuthenticated = response?.IsAuthenticated ?? false;

            if (isAuthenticated)
            {
                StoreSession(response, callerContext);
                await OpenLobbyChannelAsync(response.SessionToken);
                loginResult = LoginResult.Success;
            }
            else
            {
                loginResult = LoginResult.InvalidCredentials;
            }
        }
        catch (FaultException<GameServiceFault> exception)
        {
            _logger.WarnFormat(
                "The server rejected the login request. FaultCode: {0}.",
                exception.Detail.Code);

            loginResult = MapLoginFault(exception.Detail.Code);
        }
        catch (FaultException exception)
        {
            _logger.Error(
                "The server reported an undeclared fault while processing the login request.",
                exception);

            loginResult = LoginResult.ServerError;
        }
        catch (TimeoutException exception)
        {
            _logger.Error(
                "The login request did not finish before the configured timeout.",
                exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Error(
                "The login request could not reach the server.",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error(
                "The authentication endpoint is missing or invalid in App.config.",
                exception);
        }

        bool isLoggedIn = loginResult == LoginResult.Success;

        if (!isLoggedIn)
        {
            ForgetSession();
        }

        return loginResult;
    }

    public async Task<LobbyResult> CreateLobbyAsync()
    {
        LobbyResult result = CreateEmptyLobbyResult(LobbyResultStatus.SessionNotFound);
        LobbyServiceClient lobbyClient = GetLobbyClient();
        bool hasSession = lobbyClient != null;

        if (hasSession)
        {
            result = await SendCreateLobbyAsync(lobbyClient).ConfigureAwait(false);
        }

        return result;
    }

    public async Task<LobbyResult> JoinLobbyAsync(string code)
    {
        LobbyResult result = CreateEmptyLobbyResult(LobbyResultStatus.SessionNotFound);
        LobbyServiceClient lobbyClient = GetLobbyClient();
        bool hasSession = lobbyClient != null;

        if (hasSession)
        {
            result = await SendJoinLobbyAsync(lobbyClient, code).ConfigureAwait(false);
        }

        return result;
    }

    public async Task<LobbyResultStatus> LeaveLobbyAsync()
    {
        LobbyResultStatus resultStatus = LobbyResultStatus.SessionNotFound;
        LobbyServiceClient lobbyClient = GetLobbyClient();
        bool hasSession = lobbyClient != null;

        if (hasSession)
        {
            resultStatus = await SendLeaveLobbyAsync(lobbyClient).ConfigureAwait(false);
            ClearCurrentLobby();
        }

        return resultStatus;
    }

    public async Task<SignUpResult> SignUpAsync(string username, string email, string password)
    {
        SignUpResult signUpResult = SignUpResult.ServerUnavailable;

        try
        {
            signUpResult = await SendSignUpAsync(username, email, password).ConfigureAwait(false);
        }
        catch (FaultException<GameServiceFault> exception)
        {
            _logger.WarnFormat("The server rejected the sign up request. FaultCode: {0}.", exception.Detail.Code);
            signUpResult = MapSignUpFault(exception.Detail.Code);
        }
        catch (FaultException exception)
        {
            _logger.Error("The server reported an undeclared fault while processing the sign up request.", exception);
            signUpResult = SignUpResult.ServerError;
        }
        catch (TimeoutException exception)
        {
            _logger.Error("The sign up request did not finish before the configured timeout.", exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Error("The sign up request could not reach the server.", exception);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error("The authentication endpoint is missing or invalid in App.config.", exception);
        }

        return signUpResult;
    }

    private async Task<LobbyResult> SendCreateLobbyAsync(LobbyServiceClient lobbyClient)
    {
        LobbyResult result;

        try
        {
            LobbyResponse response = await lobbyClient.CreateLobbyAsync().ConfigureAwait(false);
            result = MapLobbyResponse(response);

            if (result.Status == LobbyResultStatus.Success)
            {
                result = SetCurrentLobby(result);
            }
        }
        catch (FaultException<GameServiceFault> exception)
        {
            result = CreateLobbyFaultResult(exception.Detail.Code);
        }
        catch (FaultException exception)
        {
            _logger.Error("The server reported an undeclared fault while creating the lobby.", exception);
            result = CreateEmptyLobbyResult(LobbyResultStatus.ServerError);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn("The lobby could not be created before the configured timeout.", exception);
            result = CreateEmptyLobbyResult(LobbyResultStatus.ServerUnavailable);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn("The lobby could not be created because of a communication error.", exception);
            result = CreateEmptyLobbyResult(LobbyResultStatus.ServerUnavailable);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error("The lobby endpoint is missing or invalid in App.config.", exception);
            result = CreateEmptyLobbyResult(LobbyResultStatus.ServerError);
        }

        return result;
    }

    private async Task<LobbyResult> SendJoinLobbyAsync(LobbyServiceClient lobbyClient, string code)
    {
        LobbyResult result;

        try
        {
            JoinLobbyRequest request = new JoinLobbyRequest
            {
                Code = code
            };
            LobbyResponse response = await lobbyClient.JoinLobbyAsync(request).ConfigureAwait(false);
            result = MapLobbyResponse(response);

            if (result.Status == LobbyResultStatus.Success)
            {
                result = SetCurrentLobby(result);
            }
        }
        catch (FaultException<GameServiceFault> exception)
        {
            result = CreateLobbyFaultResult(exception.Detail.Code);
        }
        catch (FaultException exception)
        {
            _logger.Error("The server reported an undeclared fault while joining the lobby.", exception);
            result = CreateEmptyLobbyResult(LobbyResultStatus.ServerError);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn("The lobby join request did not finish before the configured timeout.", exception);
            result = CreateEmptyLobbyResult(LobbyResultStatus.ServerUnavailable);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn("The lobby join request failed because of a communication error.", exception);
            result = CreateEmptyLobbyResult(LobbyResultStatus.ServerUnavailable);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error("The lobby endpoint is missing or invalid in App.config.", exception);
            result = CreateEmptyLobbyResult(LobbyResultStatus.ServerError);
        }

        return result;
    }

    private async Task<LobbyResultStatus> SendLeaveLobbyAsync(LobbyServiceClient lobbyClient)
    {
        LobbyResultStatus resultStatus = LobbyResultStatus.Success;

        try
        {
            await lobbyClient.LeaveLobbyAsync().ConfigureAwait(false);
        }
        catch (FaultException<GameServiceFault> exception)
        {
            resultStatus = MapLobbyFault(exception.Detail.Code);
        }
        catch (FaultException exception)
        {
            _logger.Error("The server reported an undeclared fault while leaving the lobby.", exception);
            resultStatus = LobbyResultStatus.ServerError;
        }
        catch (TimeoutException exception)
        {
            _logger.Warn("The lobby leave request did not finish before the configured timeout.", exception);
            resultStatus = LobbyResultStatus.ServerUnavailable;
        }
        catch (CommunicationException exception)
        {
            _logger.Warn("The lobby leave request failed because of a communication error.", exception);
            resultStatus = LobbyResultStatus.ServerUnavailable;
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error("The lobby endpoint is missing or invalid in App.config.", exception);
            resultStatus = LobbyResultStatus.ServerError;
        }

        return resultStatus;
    }

    private void ClearCurrentLobby()
    {
        lock (_sessionSyncRoot)
        {
            _currentLobby = null;
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
            case GameServiceFaultCode.LobbyNotFound:
                resultStatus = LobbyResultStatus.LobbyNotFound;
                break;
            case GameServiceFaultCode.LobbyFull:
                resultStatus = LobbyResultStatus.LobbyFull;
                break;
            case GameServiceFaultCode.LobbyConflict:
                resultStatus = LobbyResultStatus.AlreadyInLobby;
                break;
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

    private static LobbyResult CreateLobbyFaultResult(GameServiceFaultCode faultCode)
    {
        LobbyResultStatus status = MapLobbyFault(faultCode);
        return CreateEmptyLobbyResult(status);
    }

    private static LobbyResult CreateEmptyLobbyResult(LobbyResultStatus status)
    {
        LobbyResult result = new LobbyResult(status, string.Empty, 0, 0, 0, Array.Empty<LobbyPlayerResult>());
        return result;
    }

    private static LobbyResult MapLobbyResponse(LobbyResponse response)
    {
        LobbyResult result;

        if (response == null)
        {
            result = CreateEmptyLobbyResult(LobbyResultStatus.ServerError);
        }
        else
        {
            List<LobbyPlayerResult> players = new List<LobbyPlayerResult>();
            int hostPlayerId = NoHostPlayerId;

            foreach (LobbyPlayer player in response.Players ?? Array.Empty<LobbyPlayer>())
            {
                if (player != null)
                {
                    players.Add(new LobbyPlayerResult(
                        player.PlayerId,
                        player.Username,
                        player.IsHost));

                    if (player.IsHost)
                    {
                        hostPlayerId = player.PlayerId;
                    }
                }
            }

            result = new LobbyResult(
                LobbyResultStatus.Success,
                response.Code,
                response.MaximumPlayers,
                response.Version,
                hostPlayerId,
                players);
        }

        return result;
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

    private LobbyServiceClient GetLobbyClient()
    {
        LobbyServiceClient lobbyClient;

        lock (_sessionSyncRoot)
        {
            lobbyClient = _lobbyClient;
        }

        return lobbyClient;
    }

    private void OnPlayerJoined(object sender, PlayerJoinedEventArgs eventArgs)
    {
        bool wasApplied;

        lock (_sessionSyncRoot)
        {
            wasApplied = _currentLobby != null && eventArgs.Version > _currentLobby.Version;

            if (_currentLobby == null)
            {
                _pendingLobbyUpdates.Add(lobby => AddPlayer(lobby, eventArgs));
            }
            else if (wasApplied)
            {
                _currentLobby = AddPlayer(_currentLobby, eventArgs);
            }
        }

        if (wasApplied)
        {
            PlayerJoined?.Invoke(this, eventArgs);
        }
    }

    private void OnPlayerLeft(object sender, PlayerLeftEventArgs eventArgs)
    {
        bool wasApplied;

        lock (_sessionSyncRoot)
        {
            wasApplied = _currentLobby != null && eventArgs.Version > _currentLobby.Version;

            if (_currentLobby == null)
            {
                _pendingLobbyUpdates.Add(lobby => RemovePlayer(lobby, eventArgs));
            }
            else if (wasApplied)
            {
                _currentLobby = RemovePlayer(_currentLobby, eventArgs);
            }
        }

        if (wasApplied)
        {
            PlayerLeft?.Invoke(this, eventArgs);
        }
    }

    private static LobbyResult AddPlayer(LobbyResult lobby, PlayerJoinedEventArgs eventArgs)
    {
        LobbyResult updatedLobby = lobby;
        bool isNewerVersion = eventArgs.Version > lobby.Version;

        if (isNewerVersion)
        {
            updatedLobby = CreateLobbyWithPlayer(lobby, eventArgs);
        }

        return updatedLobby;
    }

    private static LobbyResult CreateLobbyWithPlayer(LobbyResult lobby, PlayerJoinedEventArgs eventArgs)
    {
        List<LobbyPlayerResult> players = new List<LobbyPlayerResult>(lobby.Players);
        bool alreadyPresent = players.Exists(player => player.PlayerId == eventArgs.Player.PlayerId);

        if (!alreadyPresent)
        {
            players.Add(eventArgs.Player);
        }

        LobbyResult updatedLobby = new LobbyResult(
            LobbyResultStatus.Success,
            lobby.Code,
            lobby.MaximumPlayers,
            eventArgs.Version,
            lobby.HostPlayerId,
            players);
        return updatedLobby;
    }

    private static LobbyResult RemovePlayer(LobbyResult lobby, PlayerLeftEventArgs eventArgs)
    {
        LobbyResult updatedLobby = lobby;
        bool isNewerVersion = eventArgs.Version > lobby.Version;

        if (isNewerVersion)
        {
            updatedLobby = CreateLobbyWithoutPlayer(lobby, eventArgs);
        }

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
                players.Add(new LobbyPlayerResult(player.PlayerId, player.Username, isHost));
            }
        }

        LobbyResult updatedLobby = new LobbyResult(
            LobbyResultStatus.Success,
            lobby.Code,
            lobby.MaximumPlayers,
            eventArgs.Version,
            eventArgs.HostPlayerId,
            players);
        return updatedLobby;
    }

    private GameClientCallback CreateCallback()
    {
        GameClientCallback callback = new GameClientCallback();
        callback.PlayerJoined += OnPlayerJoined;
        callback.PlayerLeft += OnPlayerLeft;
        return callback;
    }

    private void StoreSession(LoginResponse response, SynchronizationContext callerContext)
    {
        lock (_sessionSyncRoot)
        {
            _synchronizationContext = callerContext;
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

        channel.Faulted += (sender, e) => OnLobbyChannelStateChanged(lobbyClient, e, "Faulted");
        channel.Closed += (sender, e) => OnLobbyChannelStateChanged(lobbyClient, e, "Closed");

        lock (_sessionSyncRoot)
        {
            _lobbyClient = lobbyClient;
        }
    }

    private void OnLobbyChannelStateChanged(object sender, EventArgs eventArgs, string channelEvent)
    {
        bool isCurrentChannel;
        bool canReconnect = false;
        string lobbyCode = string.Empty;

        // Logged before clearing the state so PlayerId is still available.
        _logger.WarnFormat(
            "Lobby channel event: {0}. Process: {1}. PlayerId: {2}. Stack: {3}",
            channelEvent,
            _processId,
            CurrentPlayerId,
            Environment.StackTrace);

        lock (_sessionSyncRoot)
        {
            isCurrentChannel = ReferenceEquals(sender, _lobbyClient);

            if (isCurrentChannel)
            {
                canReconnect = !string.IsNullOrEmpty(_sessionToken);
                lobbyCode = _currentLobby?.Code ?? string.Empty;

                _lobbyClient = null;
                _currentLobby = null;
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

        // The new duplex channel must be created from the UI thread so its callbacks arrive there.
        if (context == null)
        {
            StartReconnection(lobbyCode);
        }
        else
        {
            context.Post(StartReconnection, lobbyCode);
        }
    }

    private void StartReconnection(object state)
    {
        string lobbyCode = state as string ?? string.Empty;

        // The channel events are synchronous, so nobody can await this task; ReconnectAsync handles
        // every expected failure itself and reports the outcome through ConnectionStateChanged.
        _ = ReconnectAsync(lobbyCode);
    }

    private async Task ReconnectAsync(string lobbyCode)
    {
        OnConnectionStateChanged(new ConnectionStateChangedEventArgs(ConnectionState.Reconnecting));

        ReconnectResult reconnectResult = ReconnectResult.Unavailable;
        int attemptCount = 0;

        // The awaits keep the UI context so every attempt creates its channel on the UI thread.
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
            LobbyResult rejoinResult = await JoinLobbyAsync(lobbyCode);
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
