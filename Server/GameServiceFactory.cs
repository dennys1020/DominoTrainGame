using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Connection;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Persistence.Data;
using DominoTrainGame.Server.Persistence.Data.Repositories;
using DominoTrainGame.Server.Service.Implementation;
using DominoTrainGame.Server.Service.Networking;

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

        ConnectedPlayerRegistry connectedPlayers = new ConnectedPlayerRegistry();
        IGameNotifier gameNotifier = new CallbackGameNotifier(connectedPlayers);
        ILobbyCodeGenerator lobbyCodeGenerator = new LobbyCodeGenerator();
        LobbyRegistry lobbyRegistry = new LobbyRegistry(lobbyCodeGenerator);
        ILobbyUseCases lobbyUseCases = new LobbyUseCases(lobbyRegistry, gameNotifier, lobbyCodeGenerator);
        SessionTokenRegistry sessionTokens = new SessionTokenRegistry();
        SessionManager sessionManager = new SessionManager(connectedPlayers, lobbyUseCases, sessionTokens);
        AuthenticationController authenticationController = new AuthenticationController(
            loginUseCases,
            signUpUseCases,
            sessionManager);
        LobbyController lobbyController = new LobbyController(lobbyUseCases, sessionManager);

        GameService gameService = new GameService(connectionUseCases, authenticationController, lobbyController);
        return gameService;
    }
}
