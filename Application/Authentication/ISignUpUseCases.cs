namespace DominoTrainGame.Server.Application.Authentication;

public interface ILoginUseCases
{
    bool SignUp(string username,string email, string password);
}
