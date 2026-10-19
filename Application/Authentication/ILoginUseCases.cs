using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.Authentication;

public interface ILoginUseCases
{
    LoginResult LogIn(string identifier, string password);
}
