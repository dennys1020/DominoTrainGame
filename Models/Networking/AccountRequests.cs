using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class AccountRequests
{
    private const string ConnectionOperationName = "connection";
    private const string LoginOperationName = "login";
    private const string SignUpOperationName = "sign up";

    private static readonly ITraceLog _logger;
    private static readonly ServiceFailureMapping<ConnectionCheckResult> _connectionFailures;
    private static readonly ServiceFailureMapping<LoginResult> _loginFailures;
    private static readonly ServiceFailureMapping<SignUpResult> _signUpFailures;

    private readonly IGameConnectionFactory _connectionFactory;
    private readonly GameChannelCloser _channelCloser;
    private readonly ClientSession _session;
    private readonly LobbyChannel _lobbyChannel;

    static AccountRequests()
    {
        _logger = TraceLogManager.GetLogger(typeof(AccountRequests));
        _connectionFailures = new ServiceFailureMapping<ConnectionCheckResult>(
            FaultCodeMapper.MapConnectionFault,
            ConnectionCheckResult.ServerError,
            ConnectionCheckResult.ServerUnavailable);
        _loginFailures = new ServiceFailureMapping<LoginResult>(
            FaultCodeMapper.MapLoginFault,
            LoginResult.ServerError,
            LoginResult.ServerUnavailable);
        _signUpFailures = new ServiceFailureMapping<SignUpResult>(
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
    }

    public async Task<ConnectionCheckResult> CheckConnectionAsync()
    {
        ConnectionCheckResult connectionResult = await SendServiceRequestAsync(
            SendCheckConnectionAsync,
            _connectionFailures,
            ConnectionOperationName).ConfigureAwait(false);
        return connectionResult;
    }

    public async Task<SignUpResult> SignUpAsync(string username, string email, string password)
    {
        SignUpResult signUpResult = await SendServiceRequestAsync(
            () => SendSignUpAsync(username, email, password),
            _signUpFailures,
            SignUpOperationName).ConfigureAwait(false);
        return signUpResult;
    }

    public async Task<LoginResult> LogInAsync(string identifier, string password)
    {
        SynchronizationContext callerContext = SynchronizationContext.Current;
        LoginResult loginResult = await SendServiceRequestAsync(
            () => AuthenticateAsync(identifier, password, callerContext),
            _loginFailures,
            LoginOperationName);

        if (loginResult != LoginResult.Success)
        {
            _session.Forget();
        }

        return loginResult;
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
