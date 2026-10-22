using System;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.Networking;
using AuthenticationServiceClient = Models.DominoService.AuthenticationServiceClient;
using ConnectionServiceClient = Models.DominoService.ConnectionServiceClient;
using GameServiceFault = Models.DominoService.GameServiceFault;
using GameServiceFaultCode = Models.DominoService.GameServiceFaultCode;
using GeneratedConnectionStatus = Models.DominoService.ServerConnectionStatus;
using GeneratedLoginRequest = Models.DominoService.LoginRequest;
using GeneratedSignUpRequest = Models.DominoService.SignUpRequest;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteGameServices : IRemoteGameServices
{
    private const string ConnectionEndpointName = "NetTcpBinding_IConnectionService";
    private const string AuthenticationEndpointName = "NetTcpBinding_IAuthenticationService";
    private static readonly ITraceLog _logger;
    private readonly GameChannelCloser _channelCloser;

    static RemoteGameServices()
    {
        _logger = TraceLogManager.GetLogger(typeof(RemoteGameServices));
    }

    public RemoteGameServices()
    {
        _channelCloser = new GameChannelCloser();
    }

    public async Task<ServerConnectionStatus> CheckConnectionAsync()
    {
        ServerConnectionStatus connectionStatus = ServerConnectionStatus.ServerUnavailable;
        ConnectionServiceClient connectionClient = new ConnectionServiceClient(ConnectionEndpointName);

        try
        {
            GeneratedConnectionStatus serverStatus =
                await connectionClient.CheckConnectionAsync().ConfigureAwait(false);
            connectionStatus = MapConnectionStatus(serverStatus);
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
        finally
        {
            _channelCloser.Close(connectionClient);
        }

        return connectionStatus;
    }

    public async Task<LoginResult> LogInAsync(string identifier, string password)
    {
        LoginResult loginResult = LoginResult.ServerUnavailable;

        try
        {
            loginResult = await SendLogInAsync(identifier, password).ConfigureAwait(false);
        }
        catch (FaultException<GameServiceFault> exception)
        {
            _logger.WarnFormat("The server rejected the login request. FaultCode: {0}.", exception.Detail.Code);
            loginResult = MapLoginFault(exception.Detail.Code);
        }
        catch (FaultException exception)
        {
            _logger.Error("The server reported an undeclared fault while processing the login request.", exception);
            loginResult = LoginResult.ServerError;
        }
        catch (TimeoutException exception)
        {
            _logger.Error("The login request did not finish before the configured timeout.", exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Error("The login request could not reach the server.", exception);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error("The authentication endpoint is missing or invalid in App.config.", exception);
        }

        return loginResult;
    }

    private async Task<SignUpResult> SendSignUpAsync(string username, string email, string password)
    {
        AuthenticationServiceClient authenticationClient = new AuthenticationServiceClient(AuthenticationEndpointName);
        SignUpResult signUpResult;

        try
        {
            GeneratedSignUpRequest request = new GeneratedSignUpRequest
            {
                UserName = username,
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

    private static ServerConnectionStatus MapConnectionStatus(GeneratedConnectionStatus serverStatus)
    {
        bool isConnected = serverStatus == GeneratedConnectionStatus.Connected;
        ServerConnectionStatus connectionStatus = isConnected
            ? ServerConnectionStatus.Connected
            : ServerConnectionStatus.DatabaseUnavailable;
        return connectionStatus;
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
            default:
                loginResult = LoginResult.ServerError;
                break;
        }

        return loginResult;
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

    private async Task<LoginResult> SendLogInAsync(string identifier, string password)
    {
        AuthenticationServiceClient authenticationClient = new AuthenticationServiceClient(AuthenticationEndpointName);
        LoginResult loginResult;

        try
        {
            GeneratedLoginRequest request = new GeneratedLoginRequest
            {
                Identifier = identifier,
                Password = password
            };

            bool isAuthenticated = await authenticationClient.LogInAsync(request).ConfigureAwait(false);
            loginResult = isAuthenticated
                ? LoginResult.Success
                : LoginResult.InvalidCredentials;
        }
        finally
        {
            _channelCloser.Close(authenticationClient);
        }

        return loginResult;
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
}
