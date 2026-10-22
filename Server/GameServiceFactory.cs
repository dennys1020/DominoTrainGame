using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Connection;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Persistence.Data;
using DominoTrainGame.Server.Persistence.Data.Repositories;
using DominoTrainGame.Server.Service.Implementation;

namespace DominoTrainGame.Server.Host;

internal static class GameServiceFactory
{
    public static GameService Create()
    {
        DatabaseExecutor databaseExecutor = new DatabaseExecutor();
        IConnectionRepository connectionRepository = new ConnectionRepository(databaseExecutor);
        IPlayerRepository playerRepository = new PlayerRepository(databaseExecutor);
        IPasswordHasher passwordHasher = new PasswordHasher();

        IConnectionUseCases connectionUseCases = new ConnectionUseCases(connectionRepository);
        ILoginUseCases loginUseCases = new LoginUseCases(playerRepository, passwordHasher);
        ISignUpUseCases signUpUseCases = new SignUpUseCases(playerRepository, passwordHasher);

        GameService gameService = new GameService(connectionUseCases, loginUseCases, signUpUseCases);
        return gameService;
    }
}
