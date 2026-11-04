using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class SocialConnectionFactory : ISocialConnectionFactory
{
    private const string FriendEndpointName = "NetTcpBinding_IFriendService";

    public FriendServiceClient CreateFriendClient()
    {
        FriendServiceClient friendClient = new FriendServiceClient(FriendEndpointName);
        return friendClient;
    }
}
