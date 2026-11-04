using System;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Connection;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Implementation;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class GameService : IConnectionService, IAuthenticationService, ILobbyService
{
    private static readonly ITraceLog _logger;

    private readonly IConnectionUseCases _connectionUseCases;
    private readonly AuthenticationController _authenticationController;
    private readonly LobbyController _lobbyController;

    static GameService()
    {
        _logger = TraceLogManager.GetLogger(typeof(GameService));
    }

    public GameService(
        IConnectionUseCases connectionUseCases,
        AuthenticationController authenticationController,
        LobbyController lobbyController)
    {
        _connectionUseCases = connectionUseCases ?? throw new ArgumentNullException(nameof(connectionUseCases));
        _authenticationController = authenticationController
            ?? throw new ArgumentNullException(nameof(authenticationController));
        _lobbyController = lobbyController ?? throw new ArgumentNullException(nameof(lobbyController));
    }

    public void CheckConnection()
    {
        try
        {
            _connectionUseCases.VerifyDatabaseConnection();
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("A connection check failed because the database is unavailable.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.DatabaseUnavailable);
        }
    }

    public LoginResponse LogIn(LoginRequest request)
    {
        LoginResponse response = _authenticationController.LogIn(request);
        return response;
    }

    public bool SignUp(SignUpRequest request)
    {
        bool isRegistered = _authenticationController.SignUp(request);
        return isRegistered;
    }

    public void Connect(ConnectRequest request)
    {
        _lobbyController.Connect(request);
    }

    public LobbyResponse CreateLobby()
    {
        LobbyResponse response = _lobbyController.CreateLobby();
        return response;
    }

    public LobbyResponse JoinLobby(JoinLobbyRequest request)
    {
        LobbyResponse response = _lobbyController.JoinLobby(request);
        return response;
    }

    public void LeaveLobby()
    {
        _lobbyController.LeaveLobby();
    }

    public void SetReady(SetReadyRequest request)
    {
        _lobbyController.SetReady(request);
    }

    public void SendLobbyMessage(SendLobbyMessageRequest request)
    {
        _lobbyController.SendLobbyMessage(request);
    }

    public void StartMatch()
    {
        _lobbyController.StartMatch();
    }
}
