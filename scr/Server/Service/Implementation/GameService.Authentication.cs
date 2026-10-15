using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed partial class GameService : IAuthenticationService
{
    private readonly LoginValidator _loginValidator;
    private readonly SignUpValidator _signUpValidator;

    public bool LogIn(string identifier, string password)
    {
        bool canLogIn =
            _loginValidator.LogInUser(identifier, password)
            == LoginValidationStatus.Success;

        return canLogIn;
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
