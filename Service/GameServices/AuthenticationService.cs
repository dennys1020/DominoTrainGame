using System;
using System.ServiceModel;
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
    private readonly ILoginUseCases _loginUseCases;
    private readonly ISignUpUseCases _signUpUseCases;
    private readonly PlayerSessionManager<IGameClientCallback> _lobbySessionManager;
    private readonly ServiceFaultTranslator _faultTranslator;

    public AuthenticationService(
        ILoginUseCases loginUseCases,
        ISignUpUseCases signUpUseCases,
        PlayerSessionManager<IGameClientCallback> lobbySessionManager,
        ServiceFaultTranslator faultTranslator)
    {
        _loginUseCases = loginUseCases ?? throw new ArgumentNullException(nameof(loginUseCases));
        _signUpUseCases = signUpUseCases ?? throw new ArgumentNullException(nameof(signUpUseCases));
        _lobbySessionManager = lobbySessionManager ?? throw new ArgumentNullException(nameof(lobbySessionManager));
        _faultTranslator = faultTranslator ?? throw new ArgumentNullException(nameof(faultTranslator));
    }

    public LoginResponse LogIn(LoginRequest request)
    {
        PlayerIdentity player;

        try
        {
            player = _loginUseCases.LogIn(request?.Identifier, request?.Password);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        LoginResponse response = player.IsEmpty
            ? CreateRejectedLoginResponse()
            : CreateLoginResponse(player);

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
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        return isRegistered;
    }

    private static LoginResponse CreateRejectedLoginResponse()
    {
        LoginResponse response = new LoginResponse
        {
            IsAuthenticated = false,
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
            SessionToken = sessionToken
        };

        return response;
    }
}
