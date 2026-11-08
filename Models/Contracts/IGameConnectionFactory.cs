using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IGameConnectionFactory
{
    ConnectionServiceClient CreateConnectionClient();

    AuthenticationServiceClient CreateAuthenticationClient();

    LobbyServiceClient CreateLobbyClient(GameClientCallback callback);

    FriendServiceClient CreateFriendClient(SocialClientCallback callback);
}
