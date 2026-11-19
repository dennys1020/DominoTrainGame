using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;
using DominoTrainGame.Client.Models.Resources.Localization;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteAuthenticationServices : IRemoteAuthenticationServices
{
    private const string ConnectionOperationName = "connection";
    private const string LoginOperationName = "login";
    private const string SignUpOperationName = "sign up";
    private const string VerifyEmailOperationName = "verify email";
    private const string ResendCodeOperationName = "resend verification code";

    private static readonly ServiceFailureMapping<ConnectionCheckResult> _connectionFailures;
    private static readonly ServiceFailureMapping<LoginResult> _loginFailures;
    private static readonly ServiceFailureMapping<SignUpResult> _signUpFailures;
    private static readonly ServiceFailureMapping<EmailVerificationResult> _verifyEmailFailures;
    private static readonly ServiceFailureMapping<EmailVerificationResult> _resendCodeFailures;

    private readonly IGameConnectionFactory _connectionFactory;
    private readonly GameChannelCloser _channelCloser;
    private readonly LobbyChannel _lobbyChannel;
    private readonly ClientSession _session;
    private readonly RemoteCallGuard _callGuard;

    static RemoteAuthenticationServices()
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
        _verifyEmailFailures = new ServiceFailureMapping<EmailVerificationResult>(
            VerifyEmailOperationName,
            FaultCodeMapper.MapEmailVerificationFault,
            EmailVerificationResult.ServerError,
            EmailVerificationResult.ServerUnavailable);
        _resendCodeFailures = new ServiceFailureMapping<EmailVerificationResult>(
            ResendCodeOperationName,
            FaultCodeMapper.MapEmailVerificationFault,
            EmailVerificationResult.ServerError,
            EmailVerificationResult.ServerUnavailable);
    }

    public RemoteAuthenticationServices(
        IGameConnectionFactory connectionFactory,
        GameChannelCloser channelCloser,
        LobbyChannel lobbyChannel,
        ClientSession session)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _channelCloser = channelCloser ?? throw new ArgumentNullException(nameof(channelCloser));
        _lobbyChannel = lobbyChannel ?? throw new ArgumentNullException(nameof(lobbyChannel));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _callGuard = new RemoteCallGuard();
    }

    public string PendingVerificationEmail => _session.PendingVerificationEmail;

    private static string CurrentLanguage => (UiStrings.Culture ?? CultureInfo.CurrentUICulture).Name;

    public bool IsGuest => _session.IsGuest;

    public Task<ConnectionCheckResult> CheckConnectionAsync()
    {
        Task<ConnectionCheckResult> connectionCheck = _callGuard.ExecuteAsync(
            SendCheckConnectionAsync,
            _connectionFailures);
        return connectionCheck;
    }

    public Task<LoginResult> LogInAsync(string identifier, string password)
    {
        LoginAttempt loginAttempt = new LoginAttempt(this, identifier, password, SynchronizationContext.Current);
        Task<LoginResult> login = RunLoginAsync(loginAttempt.AuthenticateAsync);
        return login;
    }

    public Task<LoginResult> LogInAsGuestAsync()
    {
        GuestLoginAttempt guestLoginAttempt = new GuestLoginAttempt(this, SynchronizationContext.Current);
        Task<LoginResult> login = RunLoginAsync(guestLoginAttempt.AuthenticateAsync);
        return login;
    }

    public void LogOut()
    {
        _lobbyChannel.Close();
        _session.ClearLobby();
        _session.Forget();
    }

    public Task<SignUpResult> SignUpAsync(string username, string email, string password)
    {
        SignUpAttempt signUpAttempt = new SignUpAttempt(this, username, email, password);
        Task<SignUpResult> signUp = _callGuard.ExecuteAsync(signUpAttempt.SendAsync, _signUpFailures);
        return signUp;
    }

    public Task<EmailVerificationResult> VerifyEmailAsync(string verificationCode)
    {
        Task<EmailVerificationResult> verifying = _callGuard.ExecuteAsync(
            () => SendVerifyEmailAsync(verificationCode),
            _verifyEmailFailures);
        return verifying;
    }

    public Task<EmailVerificationResult> ResendVerificationCodeAsync()
    {
        Task<EmailVerificationResult> resending = _callGuard.ExecuteAsync(
            SendResendVerificationCodeAsync,
            _resendCodeFailures);
        return resending;
    }

    private async Task<LoginResult> RunLoginAsync(Func<Task<LoginResult>> authenticate)
    {
        LoginResult loginResult = await _callGuard.ExecuteAsync(authenticate, _loginFailures);

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
        LoginResponse response = await SendLogInAsync(identifier, password);
        bool isAuthenticated = await OpenSessionAsync(response, callerContext);
        LoginResult loginResult = isAuthenticated ? LoginResult.Success : LoginResult.InvalidCredentials;

        return loginResult;
    }

    private async Task<LoginResult> AuthenticateGuestAsync(SynchronizationContext callerContext)
    {
        LoginResult loginResult = LoginResult.GuestUnavailable;
        LoginResponse response = await SendLogInAsGuestAsync();
        bool isAuthenticated = await OpenSessionAsync(response, callerContext);

        if (isAuthenticated)
        {
            _session.MarkAsGuest();
            loginResult = LoginResult.Success;
        }

        return loginResult;
    }

    private async Task<bool> OpenSessionAsync(LoginResponse response, SynchronizationContext callerContext)
    {
        bool isAuthenticated = response?.IsAuthenticated ?? false;

        if (isAuthenticated)
        {
            _session.Store(response.PlayerId, response.SessionToken, callerContext);
            await _lobbyChannel.OpenAsync(response.SessionToken);
        }

        return isAuthenticated;
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

    private async Task<LoginResponse> SendLogInAsGuestAsync()
    {
        AuthenticationServiceClient authenticationClient = _connectionFactory.CreateAuthenticationClient();
        LoginResponse response;

        try
        {
            response = await authenticationClient.LogInAsGuestAsync().ConfigureAwait(false);
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
                Password = password,
                Language = CurrentLanguage
            };

            bool isRegistered = await authenticationClient.SignUpAsync(request).ConfigureAwait(false);
            signUpResult = isRegistered
                ? SignUpResult.Success
                : SignUpResult.AlreadyExists;

            if (isRegistered)
            {
                _session.StorePendingVerificationEmail(email);
            }
        }
        finally
        {
            _channelCloser.Close(authenticationClient);
        }

        return signUpResult;
    }

    private async Task<EmailVerificationResult> SendVerifyEmailAsync(string verificationCode)
    {
        AuthenticationServiceClient authenticationClient = _connectionFactory.CreateAuthenticationClient();

        try
        {
            VerifyEmailRequest request = new VerifyEmailRequest
            {
                Email = _session.PendingVerificationEmail,
                VerificationCode = verificationCode
            };

            await authenticationClient.VerifyEmailAsync(request).ConfigureAwait(false);
            _session.StorePendingVerificationEmail(string.Empty);
        }
        finally
        {
            _channelCloser.Close(authenticationClient);
        }

        return EmailVerificationResult.Success;
    }

    private async Task<EmailVerificationResult> SendResendVerificationCodeAsync()
    {
        AuthenticationServiceClient authenticationClient = _connectionFactory.CreateAuthenticationClient();

        try
        {
            ResendVerificationCodeRequest request = new ResendVerificationCodeRequest
            {
                Email = _session.PendingVerificationEmail,
                Language = CurrentLanguage
            };

            await authenticationClient.ResendVerificationCodeAsync(request).ConfigureAwait(false);
        }
        finally
        {
            _channelCloser.Close(authenticationClient);
        }

        return EmailVerificationResult.Success;
    }

    private sealed class LoginAttempt
    {
        private readonly RemoteAuthenticationServices _authenticationServices;
        private readonly string _identifier;
        private readonly string _password;
        private readonly SynchronizationContext _callerContext;

        public LoginAttempt(
            RemoteAuthenticationServices authenticationServices,
            string identifier,
            string password,
            SynchronizationContext callerContext)
        {
            _authenticationServices = authenticationServices
                ?? throw new ArgumentNullException(nameof(authenticationServices));
            _identifier = identifier;
            _password = password;
            _callerContext = callerContext;
        }

        public Task<LoginResult> AuthenticateAsync()
        {
            Task<LoginResult> authentication = _authenticationServices.AuthenticateAsync(
                _identifier,
                _password,
                _callerContext);
            return authentication;
        }
    }

    private sealed class GuestLoginAttempt
    {
        private readonly RemoteAuthenticationServices _authenticationServices;
        private readonly SynchronizationContext _callerContext;

        public GuestLoginAttempt(
            RemoteAuthenticationServices authenticationServices,
            SynchronizationContext callerContext)
        {
            _authenticationServices = authenticationServices
                ?? throw new ArgumentNullException(nameof(authenticationServices));
            _callerContext = callerContext;
        }

        public Task<LoginResult> AuthenticateAsync()
        {
            Task<LoginResult> authentication = _authenticationServices.AuthenticateGuestAsync(_callerContext);
            return authentication;
        }
    }

    private sealed class SignUpAttempt
    {
        private readonly RemoteAuthenticationServices _authenticationServices;
        private readonly string _username;
        private readonly string _email;
        private readonly string _password;

        public SignUpAttempt(
            RemoteAuthenticationServices authenticationServices,
            string username,
            string email,
            string password)
        {
            _authenticationServices = authenticationServices
                ?? throw new ArgumentNullException(nameof(authenticationServices));
            _username = username;
            _email = email;
            _password = password;
        }

        public Task<SignUpResult> SendAsync()
        {
            Task<SignUpResult> signUp = _authenticationServices.SendSignUpAsync(_username, _email, _password);
            return signUp;
        }
    }
}
