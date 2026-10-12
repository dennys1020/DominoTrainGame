using DominoTrainGame.Utils;
using DominoTrainGame.Validator;

namespace DominoTrainGame.Server.Application.Ports;

public interface IAuthenticationStore
{
    LoginValidationStatus AuthenticateUser(string usernameOrEmail, string password);
    SignUpValidationStatus SaveNewPlayer(string username, string email, string password);
    SignUpValidationStatus FindExistingPlayer(string username, string email);
    RecoverPasswordValidationStatus LookUpAccount(string identifier, out int playerId, out string email);
    RecoverPasswordValidationStatus SaveNewPassword(int playerId, string password);
}
