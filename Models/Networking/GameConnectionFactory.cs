using System;
using System.ServiceModel;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class GameConnectionFactory : IGameConnectionFactory
{
    private const string ConnectionEndpointName = "NetTcpBinding_IConnectionService";
    private const string AuthenticationEndpointName = "NetTcpBinding_IAuthenticationService";
    private const string LobbyEndpointName = "NetTcpBinding_ILobbyService";
    private const string FriendEndpointName = "NetTcpBinding_IFriendService";

    public ConnectionServiceClient CreateConnectionClient()
    {
        ConnectionServiceClient connectionClient = new ConnectionServiceClient(ConnectionEndpointName);
        return connectionClient;
    }

    public AuthenticationServiceClient CreateAuthenticationClient()
    {
        AuthenticationServiceClient authenticationClient = new AuthenticationServiceClient(AuthenticationEndpointName);
        return authenticationClient;
    }

    public LobbyServiceClient CreateLobbyClient(GameClientCallback callback)
    {
        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        InstanceContext callbackContext = new InstanceContext(callback);
        LobbyServiceClient lobbyClient = new LobbyServiceClient(callbackContext, LobbyEndpointName);
        return lobbyClient;
    }

    public FriendServiceClient CreateFriendClient(SocialClientCallback callback)
    {
        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        InstanceContext callbackContext = new InstanceContext(callback);
        FriendServiceClient friendClient = new FriendServiceClient(callbackContext, FriendEndpointName);
        return friendClient;
    }
}
