using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IRemoteSocialServices
{
    Task<FriendListResult> GetFriendsAsync();

    Task<FriendRequestListResult> GetPendingRequestsAsync();

    Task<PlayerSearchResult> SearchPlayersAsync(string usernameFragment);

    Task<FriendResultStatus> SendFriendRequestAsync(string username);

    Task<FriendResultStatus> AcceptFriendRequestAsync(int requestId);

    Task<FriendResultStatus> RejectFriendRequestAsync(int requestId);

    Task<FriendResultStatus> RemoveFriendAsync(string username);
}
