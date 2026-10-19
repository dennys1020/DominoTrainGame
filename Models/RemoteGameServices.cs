using System;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.Networking;
using AuthenticationServiceClient = Models.DominoService.AuthenticationServiceClient;
using ConnectionServiceClient = Models.DominoService.ConnectionServiceClient;
using GeneratedConnectionStatus = Models.DominoService.ServerConnectionStatus;
using GeneratedLoginRequest = Models.DominoService.LoginRequest;
using GeneratedLoginStatus = Models.DominoService.LoginStatus;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteGameServices : IRemoteGameServices
{
    private const string ConnectionEndpointName = "NetTcpBinding_IConnectionService";
    private const string AuthenticationEndpointName = "NetTcpBinding_IAuthenticationService";
    private const int CallbackTimeoutSeconds = 30;
    private static readonly ITraceLog _logger;
    private static readonly TimeSpan _callbackTimeout;
    private readonly GameChannelCloser _channelCloser;

    static RemoteGameServices()
    {
        _logger = TraceLogManager.GetLogger(typeof(RemoteGameServices));
        _callbackTimeout = TimeSpan.FromSeconds(CallbackTimeoutSeconds);
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

    private static ServerConnectionStatus MapConnectionStatus(GeneratedConnectionStatus serverStatus)
    {
        bool isConnected = serverStatus == GeneratedConnectionStatus.Connected;
        ServerConnectionStatus connectionStatus = isConnected
            ? ServerConnectionStatus.Connected
            : ServerConnectionStatus.DatabaseUnavailable;
        return connectionStatus;
    }

    private static LoginResult MapLoginStatus(GeneratedLoginStatus loginStatus)
    {
        LoginResult loginResult;

        switch (loginStatus)
        {
            case GeneratedLoginStatus.Success:
                loginResult = LoginResult.Success;
                break;
            case GeneratedLoginStatus.InvalidInput:
                loginResult = LoginResult.InvalidInput;
                break;
            case GeneratedLoginStatus.InvalidCredentials:
                loginResult = LoginResult.InvalidCredentials;
                break;
            default:
                loginResult = LoginResult.ServerError;
                break;
        }

        return loginResult;
    }

    private async Task<LoginResult> SendLogInAsync(string identifier, string password)
    {
        AuthenticationCallback callback = new AuthenticationCallback();
        AuthenticationServiceClient authenticationClient = new AuthenticationServiceClient(
            new InstanceContext(callback),
            AuthenticationEndpointName);
        LoginResult loginResult;

        try
        {
            GeneratedLoginRequest request = new GeneratedLoginRequest
            {
                RequestId = callback.RequestId,
                Identifier = identifier,
                Password = password
            };

            await authenticationClient.LogInAsync(request).ConfigureAwait(false);
            GeneratedLoginStatus loginStatus = await callback.WaitForLogInAsync(_callbackTimeout).ConfigureAwait(false);
            loginResult = MapLoginStatus(loginStatus);
        }
        finally
        {
            _channelCloser.Close(authenticationClient);
        }

        return loginResult;
    }
}
