namespace DominoTrainGame.Server.Application.Ports;

public interface IGameNotifier
{
    void NotifyPlayerJoined(PlayerJoinedNotice notice);

    void NotifyPlayerLeft(PlayerLeftNotice notice);

    void NotifyPlayerReadyChanged(PlayerReadyChangedNotice notice);

    void NotifyLobbyMessage(LobbyMessageNotice notice);
}
