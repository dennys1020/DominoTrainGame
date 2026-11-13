using System;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteGameServices : IRemoteGameServices
{
    private const string ConnectionOperationName = "connection";
    private const string LoginOperationName = "login";
    private const string SignUpOperationName = "sign up";

    private static readonly ServiceFailureMapping<ConnectionCheckResult> _connectionFailures;
    private static readonly ServiceFailureMapping<LoginResult> _loginFailures;
    private static readonly ServiceFailureMapping<SignUpResult> _signUpFailures;

    private readonly IGameConnectionFactory _connectionFactory;
    private readonly GameChannelCloser _channelCloser;
    private readonly LobbyChannel _lobbyChannel;
    private readonly ClientSession _session;
    private readonly GameClientCallback _callback;
    private readonly RemoteCallGuard _callGuard;

    static RemoteGameServices()
    {
        _connectionFailures = new ServiceFailureMapping<ConnectionCheckResult>(
            ConnectionOperationName,
            FaultCodeMapper.MapConnectionFault,
            ConnectionCheckResult.ServerError,
            ConnectionCheckResult.ServerUnavailable);
        _loginFailures = new ServiceFailureMapping<LoginResult>(
            LoginOperationName,
            FaultCodeMapper.MapLoginFault,
            LoginResult.ServerError,
            LoginResult.ServerUnavailable);
        _signUpFailures = new ServiceFailureMapping<SignUpResult>(
            SignUpOperationName,
            FaultCodeMapper.MapSignUpFault,
            SignUpResult.ServerError,
            SignUpResult.ServerUnavailable);
    }

    public RemoteGameServices(
        IGameConnectionFactory connectionFactory,
        GameChannelCloser channelCloser,
        LobbyChannel lobbyChannel,
        ClientSession session,
        GameClientCallback callback)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _channelCloser = channelCloser ?? throw new ArgumentNullException(nameof(channelCloser));
        _lobbyChannel = lobbyChannel ?? throw new ArgumentNullException(nameof(lobbyChannel));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        _callGuard = new RemoteCallGuard();
    }

    public ILobbyEvents LobbyEvents => _callback;

    public LobbyResult CurrentLobby => _session.CurrentLobby;

    public int CurrentPlayerId => _session.CurrentPlayerId;

    public string SessionToken => _session.SessionToken;

    public Task<ConnectionCheckResult> CheckConnectionAsync()
    {
        Task<ConnectionCheckResult> connectionCheck = _callGuard.ExecuteAsync(
            SendCheckConnectionAsync,
            _connectionFailures);
        return connectionCheck;
    }

    public async Task<LoginResult> LogInAsync(string identifier, string password)
    {
        LoginAttempt loginAttempt = new LoginAttempt(this, identifier, password, SynchronizationContext.Current);
        LoginResult loginResult = await _callGuard.ExecuteAsync(loginAttempt.AuthenticateAsync, _loginFailures);

        if (loginResult != LoginResult.Success)
        {
            _session.Forget();
        }

        return loginResult;
    }

    public void LogOut()
    {
        _lobbyChannel.Close();
        _session.ClearLobby();
        _session.Forget();
    }

    public Task<LobbyResult> CreateLobbyAsync()
    {
        Task<LobbyResult> creation = _lobbyChannel.CreateLobbyAsync();
        return creation;
    }

    public Task<LobbyResult> JoinLobbyAsync(string code)
    {
        Task<LobbyResult> joining = _lobbyChannel.JoinLobbyAsync(code);
        return joining;
    }

    public async Task<LobbyResultStatus> LeaveLobbyAsync()
    {
        LobbyResultStatus resultStatus = await _lobbyChannel.SendCommandAsync(SendLeaveLobbyAsync)
            .ConfigureAwait(false);

        _session.ClearLobby();
        return resultStatus;
    }

    public Task<LobbyResultStatus> SetReadyAsync(bool isReady)
    {
        SetReadyRequest request = new SetReadyRequest
        {
            IsReady = isReady
        };
        LobbyCommand<SetReadyRequest> readyCommand = new LobbyCommand<SetReadyRequest>(request, SendReadyStateAsync);
        Task<LobbyResultStatus> command = _lobbyChannel.SendCommandAsync(readyCommand.SendAsync);
        return command;
    }

    public Task<LobbyResultStatus> SendLobbyMessageAsync(string text)
    {
        SendLobbyMessageRequest request = new SendLobbyMessageRequest
        {
            Text = text
        };
        LobbyCommand<SendLobbyMessageRequest> messageCommand = new LobbyCommand<SendLobbyMessageRequest>(
            request,
            SendMessageRequestAsync);
        Task<LobbyResultStatus> command = _lobbyChannel.SendCommandAsync(messageCommand.SendAsync);
        return command;
    }

    public Task<LobbyResultStatus> StartMatchAsync()
    {
        Task<LobbyResultStatus> command = _lobbyChannel.SendCommandAsync(SendStartMatchAsync);
        return command;
    }

    public Task<LobbyResultStatus> InviteFriendAsync(string username)
    {
        FriendUsernameRequest request = new FriendUsernameRequest
        {
            Username = username
        };
        LobbyCommand<FriendUsernameRequest> invitationCommand = new LobbyCommand<FriendUsernameRequest>(
            request,
            SendInvitationAsync);
        Task<LobbyResultStatus> command = _lobbyChannel.SendCommandAsync(invitationCommand.SendAsync);
        return command;
    }

    public Task<SignUpResult> SignUpAsync(string username, string email, string password)
    {
        SignUpAttempt signUpAttempt = new SignUpAttempt(this, username, email, password);
        Task<SignUpResult> signUp = _callGuard.ExecuteAsync(signUpAttempt.SendAsync, _signUpFailures);
        return signUp;
    }

    private static Task SendLeaveLobbyAsync(LobbyServiceClient lobbyClient)
    {
        Task leaving = lobbyClient.LeaveLobbyAsync();
        return leaving;
    }

    private static Task SendStartMatchAsync(LobbyServiceClient lobbyClient)
    {
        Task starting = lobbyClient.StartMatchAsync();
        return starting;
    }

    private static Task SendReadyStateAsync(LobbyServiceClient lobbyClient, SetReadyRequest request)
    {
        Task sending = lobbyClient.SetReadyAsync(request);
        return sending;
    }

    private static Task SendMessageRequestAsync(LobbyServiceClient lobbyClient, SendLobbyMessageRequest request)
    {
        Task sending = lobbyClient.SendLobbyMessageAsync(request);
        return sending;
    }

    private static Task SendInvitationAsync(LobbyServiceClient lobbyClient, FriendUsernameRequest request)
    {
        Task sending = lobbyClient.InviteFriendAsync(request);
        return sending;
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
            _session.Store(response.PlayerId, response.SessionToken, callerContext);
            await _lobbyChannel.OpenAsync(response.SessionToken);
            loginResult = LoginResult.Success;
        }

        return loginResult;
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

    private sealed class LoginAttempt
    {
        private readonly RemoteGameServices _gameServices;
        private readonly string _identifier;
        private readonly string _password;
        private readonly SynchronizationContext _callerContext;

        public LoginAttempt(
            RemoteGameServices gameServices,
            string identifier,
            string password,
            SynchronizationContext callerContext)
        {
            _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
            _identifier = identifier;
            _password = password;
            _callerContext = callerContext;
        }

        public Task<LoginResult> AuthenticateAsync()
        {
            Task<LoginResult> authentication = _gameServices.AuthenticateAsync(_identifier, _password, _callerContext);
            return authentication;
        }
    }

    private sealed class SignUpAttempt
    {
        private readonly RemoteGameServices _gameServices;
        private readonly string _username;
        private readonly string _email;
        private readonly string _password;

        public SignUpAttempt(RemoteGameServices gameServices, string username, string email, string password)
        {
            _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
            _username = username;
            _email = email;
            _password = password;
        }

        public Task<SignUpResult> SendAsync()
        {
            Task<SignUpResult> signUp = _gameServices.SendSignUpAsync(_username, _email, _password);
            return signUp;
        }
    }

    private sealed class LobbyCommand<TRequest>
    {
        private readonly TRequest _request;
        private readonly Func<LobbyServiceClient, TRequest, Task> _send;

        public LobbyCommand(TRequest request, Func<LobbyServiceClient, TRequest, Task> send)
        {
            _request = request;
            _send = send ?? throw new ArgumentNullException(nameof(send));
        }

        public Task SendAsync(LobbyServiceClient lobbyClient)
        {
            Task sending = _send(lobbyClient, _request);
            return sending;
        }
    }
}
