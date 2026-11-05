namespace DominoTrainGame.Server.Application.Ports;

public interface IPresenceTracker
{
    bool IsOnline(int playerId);
}
