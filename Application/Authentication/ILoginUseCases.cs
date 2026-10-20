namespace DominoTrainGame.Server.Application.Authentication;

public interface ILoginUseCases
{
    bool LogIn(string identifier, string password);
}
