using System;
using System.ServiceModel;
using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed partial class GameService : IAuthenticationService
{
    private readonly LoginValidator _loginValidator;
    private readonly SignUpValidator _signUpValidator;

    public void LogIn(Guid requestId, string identifier, string password)
    {
        IAuthenticationCallback callback = OperationContext.Current.GetCallbackChannel<IAuthenticationCallback>();
        LoginStatus status = ValidateLogin(identifier, password);
        NotifyClient(() => callback.OnLogInCompleted(requestId, status));
    }

    public void RegisterUser(Guid requestId, User user)
    {
        IAuthenticationCallback callback = OperationContext.Current.GetCallbackChannel<IAuthenticationCallback>();
        RegistrationStatus status = RegisterAccount(user);
        NotifyClient(() => callback.OnRegistrationCompleted(requestId, status));
    }

    public void CheckAvailability(Guid requestId, User user)
    {
        IAuthenticationCallback callback = OperationContext.Current.GetCallbackChannel<IAuthenticationCallback>();
        RegistrationStatus status = GetAvailabilityStatus(user);
        NotifyClient(() => callback.OnAvailabilityChecked(requestId, status));
    }

    private LoginStatus ValidateLogin(string identifier, string password)
    {
        LoginValidationStatus validationStatus =
            _loginValidator.LogInUser(identifier, password);
        LoginStatus status;

        switch (validationStatus)
        {
            case LoginValidationStatus.Success:
                status = LoginStatus.Success;
                break;
            case LoginValidationStatus.EmptyFields:
                status = LoginStatus.EmptyFields;
                break;
            case LoginValidationStatus.UserNotFound:
                status = LoginStatus.UserNotFound;
                break;
            case LoginValidationStatus.IncorrectPassword:
                status = LoginStatus.IncorrectPassword;
                break;
            default:
                status = LoginStatus.SystemError;
                break;
        }

        return status;
    }

    private RegistrationStatus GetAvailabilityStatus(User user)
    {
        SignUpValidationStatus result = SignUpValidationStatus.EmptyFields;
        if (user != null)
        {
            result = _signUpValidator.CheckAvailability(user.Username, user.Email?.Trim(), user.Password);
        }

        return MapRegistrationStatus(result);
    }

    private RegistrationStatus RegisterAccount(User user)
    {
        SignUpValidationStatus result = SignUpValidationStatus.EmptyFields;

        if (user != null)
        {
            Player player = new Player
            {
                Username = user.Username,
                Email = user.Email?.Trim()
            };

            result = SignUpValidator.ValidateInput(player.Username, player.Email, user.Password);
            if (result == SignUpValidationStatus.Success)
            {
                result = _verification.ExecuteVerified(user.VerificationId, player.Email, RegisterPlayer);
            }

            SignUpValidationStatus RegisterPlayer()
            {
                return _signUpValidator.RegisterUser(player, user.Password, NormalizeLanguage(user.Language));
            }
        }

        return MapRegistrationStatus(result);
    }

    private static RegistrationStatus MapRegistrationStatus(SignUpValidationStatus result)
    {
        RegistrationStatus status;
        switch (result)
        {
            case SignUpValidationStatus.Success:
                status = RegistrationStatus.Success;
                break;
            case SignUpValidationStatus.EmptyFields:
                status = RegistrationStatus.EmptyFields;
                break;
            case SignUpValidationStatus.InvalidEmail:
                status = RegistrationStatus.InvalidEmail;
                break;
            case SignUpValidationStatus.PasswordTooShort:
                status = RegistrationStatus.PasswordTooShort;
                break;
            case SignUpValidationStatus.UserAlreadyExists:
                status = RegistrationStatus.UserAlreadyExists;
                break;
            case SignUpValidationStatus.VerificationRequired:
                status = RegistrationStatus.VerificationRequired;
                break;
            default:
                status = RegistrationStatus.SystemError;
                break;
        }

        return status;
    }
}
