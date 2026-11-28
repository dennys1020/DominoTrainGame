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

    public LoginResponse LogIn(LoginRequest request)
    {
        PlayerIdentity player = _faultTranslator.Execute(
            () => _loginUseCases.LogIn(request?.Identifier, request?.Password));

        LoginResponse response = CreateLoginResponse(player);
        return response;
    }

    public LoginResponse LogInAsGuest()
    {
        PlayerIdentity guest = _faultTranslator.Execute(_guestLoginUseCases.LogInAsGuest);
        LoginResponse response = CreateLoginResponse(guest);
        return response;
    }

    public bool SignUp(SignUpRequest request)
    {
        SignUpDetails details = new SignUpDetails(
            request?.Username,
            request?.Email,
            request?.Password);
        bool isRegistered = _faultTranslator.Execute(() => _signUpUseCases.SignUp(details, request?.Language));

        return isRegistered;
    }

    public void VerifyEmail(VerifyEmailRequest request)
    {
        VerifyEmailStatus status = _faultTranslator.Execute(
            () => _signUpUseCases.VerifyEmail(request?.Email, request?.VerificationCode));

        GameFaultFactory.ThrowIfRejected(status, VerifyEmailStatus.Success, MapVerifyEmailFaultCode);
    }

    public void ResendVerificationCode(ResendVerificationCodeRequest request)
    {
        ResendVerificationCodeStatus resendStatus = _faultTranslator.Execute(
            () => _signUpUseCases.ResendVerificationCode(request?.Email, request?.Language));

        GameFaultFactory.ThrowIfRejected(resendStatus, ResendVerificationCodeStatus.Success, MapResendFaultCode);
    }

    private static GameServiceFaultCode MapResendFaultCode(ResendVerificationCodeStatus resendStatus)
    {
        GameServiceFaultCode faultCode;

        switch (resendStatus)
        {
            case ResendVerificationCodeStatus.RegistrationNotFound:
                faultCode = GameServiceFaultCode.VerificationNotFound;
                break;
            case ResendVerificationCodeStatus.TooSoon:
                faultCode = GameServiceFaultCode.ResendTooSoon;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidInput;
                break;
        }

        return faultCode;
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
            SessionToken = string.Empty,
            Username = string.Empty
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
            SessionToken = sessionToken,
            Username = player.Username
        };

        return response;
    }
}
