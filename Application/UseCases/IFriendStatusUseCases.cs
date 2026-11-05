namespace DominoTrainGame.Server.Application.UseCases;

public interface IFriendStatusUseCases
{
    void PublishStatus(int playerId);
}
