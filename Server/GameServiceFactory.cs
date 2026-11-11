using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Security;
using DominoTrainGame.Server.Application.State;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Persistence.Data;
using DominoTrainGame.Server.Persistence.Data.Repositories;
using DominoTrainGame.Server.Persistence.Time;
using DominoTrainGame.Server.Service.Callbacks;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.Faults;
using DominoTrainGame.Server.Service.Implementation;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Host;

internal static class GameServiceFactory
{
    public static List<ServiceHost> CreateHosts()
    {
        DatabaseExecutor databaseExecutor = new DatabaseExecutor();
        IConnectionRepository connectionRepository = new ConnectionRepository(databaseExecutor);
        IPlayerRepository playerRepository = new PlayerRepository(databaseExecutor);
        IPasswordHasher passwordHasher = new PasswordHasher();

        IConnectionUseCases connectionUseCases = new ConnectionUseCases(connectionRepository);
        ILoginUseCases loginUseCases = new LoginUseCases(playerRepository, passwordHasher);
        ISignUpUseCases signUpUseCases = new SignUpUseCases(playerRepository, passwordHasher);

        PlayerSessionRegistry<IGameClientCallback> lobbySessions = new PlayerSessionRegistry<IGameClientCallback>();
        IGameNotifier gameNotifier = new CallbackGameNotifier(lobbySessions);
        ILobbyCodeGenerator lobbyCodeGenerator = new LobbyCodeGenerator();
        LobbyRegistry lobbyRegistry = new LobbyRegistry(lobbyCodeGenerator);
        IForbiddenWordSource forbiddenWordSource = new ForbiddenWordSource();
        IChatMessageFilter chatMessageFilter = new ChatMessageFilter(forbiddenWordSource);
        ISanctionRepository sanctionRepository = new SanctionRepository(databaseExecutor);
        ChatInfractionRegistry chatInfractions = new ChatInfractionRegistry();
        IClock clock = new SystemClock();
        IChatSanctionUseCases chatSanctionUseCases = new ChatSanctionUseCases(
            sanctionRepository,
            chatInfractions,
            clock);

        IFriendshipRepository friendshipRepository = new FriendshipRepository(databaseExecutor);
        PlayerSessionRegistry<ISocialClientCallback> socialSessions =
            new PlayerSessionRegistry<ISocialClientCallback>();
        ISocialNotifier socialNotifier = new CallbackSocialNotifier(socialSessions);
        IPresenceUseCases presenceUseCases = new PresenceUseCases(socialSessions, lobbyRegistry);
        IFriendUseCases friendUseCases = new FriendUseCases(friendshipRepository, presenceUseCases, socialNotifier);
        MatchRegistry matchRegistry = new MatchRegistry();
        ITileShuffler tileShuffler = new TileShuffler();
        IMatchUseCases matchUseCases = new MatchUseCases(matchRegistry, tileShuffler, gameNotifier);
        ILobbyUseCases lobbyUseCases = new LobbyUseCases(
            lobbyRegistry,
            gameNotifier,
            chatMessageFilter,
            chatSanctionUseCases,
            friendUseCases,
            matchUseCases);
        IInvitationUseCases invitationUseCases = new InvitationUseCases(
            lobbyRegistry,
            friendshipRepository,
            gameNotifier);

        SessionTokenRegistry sessionTokens = new SessionTokenRegistry();
        PlayerSessionManager<IGameClientCallback> lobbySessionManager =
            new PlayerSessionManager<IGameClientCallback>(lobbySessions, sessionTokens);
        PlayerSessionManager<ISocialClientCallback> socialSessionManager =
            new PlayerSessionManager<ISocialClientCallback>(socialSessions, sessionTokens);

        lobbySessionManager.PlayerDisconnected += (sender, eventArgs) => lobbyUseCases.LeaveLobby(eventArgs.PlayerId);
        socialSessionManager.PlayerConnected +=
            (sender, eventArgs) => friendUseCases.PublishStatus(eventArgs.PlayerId);
        socialSessionManager.PlayerDisconnected +=
            (sender, eventArgs) => friendUseCases.PublishStatus(eventArgs.PlayerId);

        ServiceFaultTranslator faultTranslator = new ServiceFaultTranslator();
        ConnectionService connectionService = new ConnectionService(connectionUseCases);
        AuthenticationService authenticationService = new AuthenticationService(
            loginUseCases,
            signUpUseCases,
            lobbySessionManager,
            faultTranslator);
        LobbyService lobbyService = new LobbyService(
            lobbyUseCases,
            invitationUseCases,
            lobbySessionManager,
            faultTranslator);
        FriendService friendService = new FriendService(friendUseCases, socialSessionManager, faultTranslator);
        MatchService matchService = new MatchService(matchUseCases, sessionTokens, faultTranslator);

        IErrorHandler errorHandler = new GameErrorHandler();
        List<ServiceHost> serviceHosts = new List<ServiceHost>
        {
            new GameServiceHost(connectionService, errorHandler),
            new GameServiceHost(authenticationService, errorHandler),
            new GameServiceHost(lobbyService, errorHandler),
            new GameServiceHost(friendService, errorHandler),
            new GameServiceHost(matchService, errorHandler)
        };

        return serviceHosts;
    }
}
