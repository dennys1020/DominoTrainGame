using System;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class AccountRequests
{
    private const string ConnectionOperationName = "connection";
    private const string LoginOperationName = "login";
    private const string SignUpOperationName = "sign up";

    private static readonly ServiceFailureMapping<ConnectionCheckResult> _connectionFailures;
    private static readonly ServiceFailureMapping<LoginResult> _loginFailures;
    private static readonly ServiceFailureMapping<SignUpResult> _signUpFailures;

    private readonly IGameConnectionFactory _connectionFactory;
    private readonly GameChannelCloser _channelCloser;
    private readonly ClientSession _session;
    private readonly LobbyChannel _lobbyChannel;
    private readonly RemoteCallGuard _callGuard;

    static AccountRequests()
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

    public AccountRequests(
        IGameConnectionFactory connectionFactory,
        GameChannelCloser channelCloser,
        ClientSession session,
        LobbyChannel lobbyChannel)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _channelCloser = channelCloser ?? throw new ArgumentNullException(nameof(channelCloser));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _lobbyChannel = lobbyChannel ?? throw new ArgumentNullException(nameof(lobbyChannel));
        _callGuard = new RemoteCallGuard();
    }

    public Task<ConnectionCheckResult> CheckConnectionAsync()
    {
        Task<ConnectionCheckResult> connectionCheck = _callGuard.ExecuteAsync(
            SendCheckConnectionAsync,
            _connectionFailures);
        return connectionCheck;
    }

    public Task<SignUpResult> SignUpAsync(string username, string email, string password)
    {
        Task<SignUpResult> signUp = _callGuard.ExecuteAsync(
            () => SendSignUpAsync(username, email, password),
            _signUpFailures);
        return signUp;
    }

    public async Task<LoginResult> LogInAsync(string identifier, string password)
    {
        SynchronizationContext callerContext = SynchronizationContext.Current;
        LoginResult loginResult = await _callGuard.ExecuteAsync(
            () => AuthenticateAsync(identifier, password, callerContext),
            _loginFailures);

        if (loginResult != LoginResult.Success)
        {
            _session.Forget();
        }

        return loginResult;
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
}
