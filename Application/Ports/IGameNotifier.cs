
namespace DominoTrainGame.Server.Application.Ports;

public interface IGameNotifier
{
    void NotifyPlayerJoined(PlayerJoinedNotice notice);
    void NotifyPlayerLeft(PlayerLeftNotice notice);
}
