using System;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed class AuthenticationController
{
    private static readonly ITraceLog _logger;

    private readonly ILoginUseCases _loginUseCases;
    private readonly ISignUpUseCases _signUpUseCases;
    private readonly SessionManager _sessionManager;

    static AuthenticationController()
    {
        _logger = TraceLogManager.GetLogger(typeof(AuthenticationController));
    }

    public AuthenticationController(
        ILoginUseCases loginUseCases,
        ISignUpUseCases signUpUseCases,
        SessionManager sessionManager)
    {
        _loginUseCases = loginUseCases ?? throw new ArgumentNullException(nameof(loginUseCases));
        _signUpUseCases = signUpUseCases ?? throw new ArgumentNullException(nameof(signUpUseCases));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
    }

    public LoginResponse LogIn(LoginRequest request)
    {
        LoginResponse response = new LoginResponse
        {
            IsAuthenticated = false,
            Username = string.Empty,
            SessionToken = string.Empty
        };

        try
        {
            bool isAuthenticated = _loginUseCases.LogIn(
                request?.Identifier,
                request?.Password,
                out AuthenticatedPlayer player);

            if (isAuthenticated)
            {
                response = CreateLoginResponse(player);
            }
        }
        catch (ArgumentException exception)
        {
            _logger.Warn("A login request was rejected because its input is invalid.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("A login request failed because the database is unavailable.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.DatabaseUnavailable);
        }

        return response;
    }

    public bool SignUp(SignUpRequest request)
    {
        bool isRegistered;

        try
        {
            isRegistered = _signUpUseCases.SignUp(request?.Username, request?.Email, request?.Password);
        }
        catch (ArgumentException exception)
        {
            _logger.Warn("A sign up request was rejected because its input is invalid.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("A sign up request failed because the database is unavailable.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.DatabaseUnavailable);
        }

        return isRegistered;
    }

    private LoginResponse CreateLoginResponse(AuthenticatedPlayer player)
    {
        string sessionToken = _sessionManager.IssueToken(player);
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
