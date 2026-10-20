using System;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Connection;
using DominoTrainGame.Server.Application.Errors;
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

    public bool LogIn(LoginRequest request)
    {
        bool isAuthenticated;

        try
        {
            isAuthenticated = _loginUseCases.LogIn(request?.Identifier, request?.Password);
        }
        catch (ArgumentException exception)
        {
            _logger.Warn("A login request was rejected because its input is invalid.", exception);
            throw CreateFault(GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("A login request failed because the database is unavailable.", exception);
            throw CreateFault(GameServiceFaultCode.DatabaseUnavailable);
        }

        return isAuthenticated;
    }

    public void SignUp(SignUpRequest request)
    {

    }

    private static FaultException<GameServiceFault> CreateFault(GameServiceFaultCode faultCode)
    {
        GameServiceFault fault = new GameServiceFault
        {
            Code = faultCode
        };
        FaultException<GameServiceFault> faultException = new FaultException<GameServiceFault>(fault);
        return faultException;
    }
}
