using System;
using System.ServiceModel;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class SocialConnectionFactory : ISocialConnectionFactory
{
    private const string FriendEndpointName = "NetTcpBinding_IFriendService";

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
