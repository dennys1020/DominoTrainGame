using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class GameConnectionFactory : IGameConnectionFactory
{
    private const string ConnectionEndpointName = "NetTcpBinding_IConnectionService";
    private const string AuthenticationEndpointName = "NetTcpBinding_IAuthenticationService";

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
}
