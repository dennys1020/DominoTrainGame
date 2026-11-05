using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Contracts;

public interface ISocialConnectionFactory
{
    FriendServiceClient CreateFriendClient(SocialClientCallback callback);
}
