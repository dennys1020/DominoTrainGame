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
    private readonly IGuestLoginUseCases _guestLoginUseCases;
    private readonly ISignUpUseCases _signUpUseCases;
    private readonly PlayerSessionManager<IGameClientCallback> _lobbySessionManager;
    private readonly ServiceFaultTranslator _faultTranslator;

    public AuthenticationService(
        ILoginUseCases loginUseCases,
        IGuestLoginUseCases guestLoginUseCases,
        ISignUpUseCases signUpUseCases,
        PlayerSessionManager<IGameClientCallback> lobbySessionManager,
        ServiceFaultTranslator faultTranslator)
    {
        _loginUseCases = loginUseCases ?? throw new ArgumentNullException(nameof(loginUseCases));
        _guestLoginUseCases = guestLoginUseCases ?? throw new ArgumentNullException(nameof(guestLoginUseCases));
        _signUpUseCases = signUpUseCases ?? throw new ArgumentNullException(nameof(signUpUseCases));
        _lobbySessionManager = lobbySessionManager ?? throw new ArgumentNullException(nameof(lobbySessionManager));
        _faultTranslator = faultTranslator ?? throw new ArgumentNullException(nameof(faultTranslator));
    }

    public LoginResponse LogIn(LoginRequest logInRequest)
    {
        PlayerIdentity player;

        try
        {
            player = _loginUseCases.LogIn(logInRequest?.Identifier, logInRequest?.Password);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        LoginResponse response = CreateLoginResponse(player);
        return response;
    }

    public LoginResponse LogInAsGuest()
    {
        PlayerIdentity guest;

        try
        {
            guest = _guestLoginUseCases.LogInAsGuest();
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        LoginResponse response = CreateLoginResponse(guest);
        return response;
    }

    public bool SignUp(SignUpRequest signUpRequest)
    {
        bool isRegistered;

        try
        {
            isRegistered = _signUpUseCases.SignUp(
                signUpRequest?.Username,
                signUpRequest?.Email,
                signUpRequest?.Password);
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
        LoginResponse response = player.IsEmpty
            ? CreateRejectedLoginResponse()
            : CreateAcceptedLoginResponse(player);

        return response;
    }

    private LoginResponse CreateAcceptedLoginResponse(PlayerIdentity player)
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
