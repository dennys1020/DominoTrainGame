namespace DominoTrainGame.Server.Application.UseCases;

public interface ISignUpUseCases
{
    bool SignUp(string username, string email, string password);
}
