namespace DominoTrainGame.Server.Application.Ports;

public interface ISocialNotifier
{
    void NotifyFriendStatusChanged(FriendStatusNotice notice);

    void NotifyFriendsChanged(int playerId);
}
