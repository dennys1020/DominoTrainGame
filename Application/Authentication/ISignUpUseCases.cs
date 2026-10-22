namespace DominoTrainGame.Server.Application.Authentication;

public interface ISignUpUseCases
{
    bool SignUp(string username, string email, string password);
}
