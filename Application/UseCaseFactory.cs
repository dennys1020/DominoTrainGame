using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Connection;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Repositories;

namespace DominoTrainGame.Server.Application;

public static class UseCaseFactory
{
    public static IConnectionUseCases CreateConnectionUseCases()
    {
        IConnectionRepository connectionRepository = new ConnectionRepository();
        IConnectionUseCases connectionUseCases = new ConnectionUseCases(connectionRepository);
        return connectionUseCases;
    }

    public static ILoginUseCases CreateLoginUseCases()
    {
        IPlayerRepository playerRepository = new PlayerRepository();
        ILoginUseCases loginUseCases = new LoginUseCases(playerRepository);
        return loginUseCases;
    }
}
