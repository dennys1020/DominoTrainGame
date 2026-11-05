using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Application.State;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Persistence.Data;
using DominoTrainGame.Server.Persistence.Data.Repositories;
using DominoTrainGame.Server.Service.Callbacks;
using DominoTrainGame.Server.Service.Implementation;
using DominoTrainGame.Server.Service.Sessions;

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

        IFriendshipRepository friendshipRepository = new FriendshipRepository(databaseExecutor);
        SocialSessionRegistry socialSessions = new SocialSessionRegistry();
        ISocialNotifier socialNotifier = new CallbackSocialNotifier(socialSessions);
        IPresenceUseCases presenceUseCases = new PresenceUseCases(socialSessions, lobbyRegistry);
        IFriendStatusUseCases friendStatusUseCases = new FriendStatusUseCases(
            friendshipRepository,
            presenceUseCases,
            socialNotifier);
        ILobbyUseCases trackedLobbyUseCases = new PresenceTrackingLobbyUseCases(
            lobbyUseCases,
            presenceUseCases,
            friendStatusUseCases);

        SessionTokenRegistry sessionTokens = new SessionTokenRegistry();
        SessionManager sessionManager = new SessionManager(connectedPlayers, trackedLobbyUseCases, sessionTokens);
        AuthenticationController authenticationController = new AuthenticationController(
            loginUseCases,
            signUpUseCases,
            sessionManager);
        IInvitationUseCases invitationUseCases = new InvitationUseCases(
            lobbyRegistry,
            friendshipRepository,
            gameNotifier);
        LobbyController lobbyController = new LobbyController(
            trackedLobbyUseCases,
            invitationUseCases,
            sessionManager);

        IFriendUseCases friendUseCases = new FriendUseCases(friendshipRepository, presenceUseCases, socialNotifier);
        SocialSessionManager socialSessionManager = new SocialSessionManager(
            sessionTokens,
            socialSessions,
            friendStatusUseCases);
        FriendController friendController = new FriendController(friendUseCases, socialSessionManager);
        SessionController sessionController = new SessionController(lobbyController, friendController);

        GameService gameService = new GameService(connectionUseCases, authenticationController, sessionController);
        return gameService;
    }
}
