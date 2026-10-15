using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed partial class GameService : IAuthenticationService
{
    private readonly LoginValidator _loginValidator;
    private readonly SignUpValidator _signUpValidator;

    public LoginStatus LogIn(string identifier, string password)
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

    public bool RegisterUser(User user)
    {
        bool isRegistered = false;

        if (user != null)
        {
            Player player = new Player
            {
                Username = user.Username,
                Email = user.Email
            };

            SignUpValidationStatus status =
                _signUpValidator.RegisterUser(
                    player,
                    user.Password,
                    NormalizeLanguage(user.Language));

            isRegistered =
                status == SignUpValidationStatus.Success;
        }

        return isRegistered;
    }
}
