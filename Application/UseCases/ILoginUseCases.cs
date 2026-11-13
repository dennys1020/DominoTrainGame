using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.UseCases;

public interface ILoginUseCases
{
    PlayerIdentity LogIn(string identifier, string password);
}
