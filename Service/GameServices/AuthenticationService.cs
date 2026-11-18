using System;
using System.ServiceModel;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;
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
        PlayerIdentity player = _faultTranslator.Execute(
            () => _loginUseCases.LogIn(logInRequest?.Identifier, logInRequest?.Password));

        LoginResponse response = CreateLoginResponse(player);
        return response;
    }

    public LoginResponse LogInAsGuest()
    {
        PlayerIdentity guest = _faultTranslator.Execute(_guestLoginUseCases.LogInAsGuest);
        LoginResponse response = CreateLoginResponse(guest);
        return response;
    }

    public bool SignUp(SignUpRequest signUpRequest)
    {
        bool isRegistered = _faultTranslator.Execute(
            () => _signUpUseCases.SignUp(signUpRequest?.Username, signUpRequest?.Email, signUpRequest?.Password));

        return isRegistered;
    }

    public void VerifyEmail(VerifyEmailRequest verifyEmailRequest)
    {
        VerifyEmailStatus status = _faultTranslator.Execute(
            () => _signUpUseCases.VerifyEmail(verifyEmailRequest?.Email, verifyEmailRequest?.VerificationCode));

        GameFaultFactory.ThrowIfRejected(status, VerifyEmailStatus.Success, MapVerifyEmailFaultCode);
    }

    public bool ResendVerificationCode(ResendVerificationCodeRequest resendRequest)
    {
        bool wasResent = _faultTranslator.Execute(
            () => _signUpUseCases.ResendVerificationCode(resendRequest?.Email));

        return wasResent;
    }

    private static GameServiceFaultCode MapVerifyEmailFaultCode(VerifyEmailStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case VerifyEmailStatus.RegistrationNotFound:
                faultCode = GameServiceFaultCode.VerificationNotFound;
                break;
            case VerifyEmailStatus.InvalidCode:
                faultCode = GameServiceFaultCode.InvalidVerificationCode;
                break;
            case VerifyEmailStatus.CodeExpired:
                faultCode = GameServiceFaultCode.VerificationCodeExpired;
                break;
            case VerifyEmailStatus.TooManyAttempts:
                faultCode = GameServiceFaultCode.TooManyVerificationAttempts;
                break;
            case VerifyEmailStatus.AlreadyRegistered:
                faultCode = GameServiceFaultCode.AccountAlreadyExists;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidInput;
                break;
        }

        return faultCode;
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
