using System;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Connection;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Implementation;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class GameService : IConnectionService, IAuthenticationService
{
    private static readonly ITraceLog _logger;
    private readonly IConnectionUseCases _connectionUseCases;
    private readonly ILoginUseCases _loginUseCases;

    static GameService()
    {
        _logger = TraceLogManager.GetLogger(typeof(GameService));
    }

    public GameService(IConnectionUseCases connectionUseCases, ILoginUseCases loginUseCases)
    {
        _connectionUseCases = connectionUseCases ?? throw new ArgumentNullException(nameof(connectionUseCases));
        _loginUseCases = loginUseCases ?? throw new ArgumentNullException(nameof(loginUseCases));
    }

    public ServerConnectionStatus CheckConnection()
    {
        DatabaseConnectionStatus databaseStatus = _connectionUseCases.CheckDatabaseConnection();
        bool isDatabaseAvailable = databaseStatus == DatabaseConnectionStatus.Available;
        ServerConnectionStatus connectionStatus = isDatabaseAvailable
            ? ServerConnectionStatus.Connected
            : ServerConnectionStatus.DatabaseUnavailable;
        return connectionStatus;
    }

    public void LogIn(LoginRequest request)
    {
        IAuthenticationCallback callback = OperationContext.Current.GetCallbackChannel<IAuthenticationCallback>();
        LoginResponse response = CreateLoginResponse(request);

        NotifyLogInCompleted(callback, response);
    }

    private LoginResponse CreateLoginResponse(LoginRequest request)
    {
        LoginResponse response = new LoginResponse
        {
            RequestId = request?.RequestId ?? Guid.Empty,
            Status = LoginStatus.InvalidInput
        };

        if (request != null)
        {
            LoginResult loginResult = _loginUseCases.LogIn(request.Identifier, request.Password);
            response.Status = MapLoginStatus(loginResult);
        }

        return response;
    }

    private static LoginStatus MapLoginStatus(LoginResult loginResult)
    {
        LoginStatus loginStatus;

        switch (loginResult)
        {
            case LoginResult.Success:
                loginStatus = LoginStatus.Success;
                break;
            case LoginResult.InvalidInput:
                loginStatus = LoginStatus.InvalidInput;
                break;
            case LoginResult.InvalidCredentials:
                loginStatus = LoginStatus.InvalidCredentials;
                break;
            default:
                loginStatus = LoginStatus.ServerError;
                break;
        }

        return loginStatus;
    }

    private static void NotifyLogInCompleted(IAuthenticationCallback callback, LoginResponse response)
    {
        try
        {
            callback.OnLogInCompleted(response);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn("The login result could not be delivered before the configured timeout.", exception);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn("The login result could not be delivered because the client disconnected.", exception);
        }
        catch (ObjectDisposedException exception)
        {
            _logger.Warn("The login result could not be delivered because the callback channel was closed.", exception);
        }
    }
}
