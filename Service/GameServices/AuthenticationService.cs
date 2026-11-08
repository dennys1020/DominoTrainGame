using System;
using System.ServiceModel;
using DominoTrainGame.Server.Application.Domain;
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
        PlayerIdentity player = PlayerIdentity.Empty;
        bool isAuthenticated = _faultTranslator.Execute(
            () => _loginUseCases.LogIn(request?.Identifier, request?.Password, out player),
            GameServiceFaultCode.InvalidInput);
        LoginResponse response = isAuthenticated
            ? CreateLoginResponse(player)
            : CreateRejectedLoginResponse();

        return response;
    }

    public bool SignUp(SignUpRequest request)
    {
        bool isRegistered = _faultTranslator.Execute(
            () => _signUpUseCases.SignUp(request?.Username, request?.Email, request?.Password),
            GameServiceFaultCode.InvalidInput);
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
