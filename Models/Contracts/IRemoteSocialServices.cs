using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IRemoteSocialServices
{
    IFriendEvents FriendEvents { get; }

    Task<FriendActionResult> ConnectAsync();

    Task<FriendListResult> GetFriendsAsync();

    Task<PendingFriendshipListResult> GetPendingRequestsAsync();

    Task<PlayerSearchResult> SearchPlayersAsync(string usernameFragment);

    Task<FriendActionResult> SendFriendRequestAsync(string username);

    Task<FriendActionResult> AcceptFriendRequestAsync(int requestId);

    Task<FriendActionResult> RejectFriendRequestAsync(int requestId);

    Task<FriendActionResult> RemoveFriendAsync(string username);

    void Disconnect();
}
