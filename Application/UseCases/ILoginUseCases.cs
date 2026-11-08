using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.UseCases;

public interface ILoginUseCases
{
    bool LogIn(string identifier, string password, out PlayerIdentity player);
}
