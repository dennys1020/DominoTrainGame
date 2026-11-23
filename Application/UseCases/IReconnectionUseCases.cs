namespace DominoTrainGame.Server.Application.UseCases;

public interface IReconnectionUseCases
{
    bool HoldSeat(int playerId);

    bool Reconnect(int playerId, string matchCode);
}
