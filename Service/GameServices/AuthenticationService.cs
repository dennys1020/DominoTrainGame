using System;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Service.Implementation;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class AuthenticationService : IAuthenticationService
{
    private static readonly ITraceLog _logger;

    private readonly ILoginUseCases _loginUseCases;
    private readonly ISignUpUseCases _signUpUseCases;
    private readonly PlayerSessionManager<IGameClientCallback> _lobbySessionManager;

    static AuthenticationService()
    {
        _logger = TraceLogManager.GetLogger(typeof(AuthenticationService));
    }

    public AuthenticationService(
        ILoginUseCases loginUseCases,
        ISignUpUseCases signUpUseCases,
        PlayerSessionManager<IGameClientCallback> lobbySessionManager)
    {
        _loginUseCases = loginUseCases ?? throw new ArgumentNullException(nameof(loginUseCases));
        _signUpUseCases = signUpUseCases ?? throw new ArgumentNullException(nameof(signUpUseCases));
        _lobbySessionManager = lobbySessionManager ?? throw new ArgumentNullException(nameof(lobbySessionManager));
    }

    public LoginResponse LogIn(LoginRequest request)
    {
        PlayerIdentity player = PlayerIdentity.Empty;
        bool isAuthenticated = ExecuteAccountRequest(
            () => _loginUseCases.LogIn(request?.Identifier, request?.Password, out player));
        LoginResponse response = isAuthenticated
            ? CreateLoginResponse(player)
            : CreateRejectedLoginResponse();

        return response;
    }

    public bool SignUp(SignUpRequest request)
    {
        bool isRegistered = ExecuteAccountRequest(
            () => _signUpUseCases.SignUp(request?.Username, request?.Email, request?.Password));
        return isRegistered;
    }

    private static TResult ExecuteAccountRequest<TResult>(Func<TResult> request)
    {
        TResult result;

        try
        {
            result = request();
        }
        catch (ArgumentException exception)
        {
            _logger.Warn("An account request was rejected because its input is invalid.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("An account request failed because the database is unavailable.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.DatabaseUnavailable);
        }

        return result;
    }

    private static LoginResponse CreateRejectedLoginResponse()
    {
        LoginResponse response = new LoginResponse
        {
            IsAuthenticated = false,
            Username = string.Empty,
            SessionToken = string.Empty
        };

        return response;
    }

    private LoginResponse CreateLoginResponse(PlayerIdentity player)
    {
        string sessionToken = _lobbySessionManager.IssueToken(player);
        LoginResponse response = new LoginResponse
        {
            IsAuthenticated = true,
            PlayerId = player.PlayerId,
            Username = player.Username,
            SessionToken = sessionToken
        };

        return response;
    }
}
