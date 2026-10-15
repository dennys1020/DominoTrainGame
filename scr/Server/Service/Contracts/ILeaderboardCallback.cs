using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

public interface ILeaderboardCallback
{
    [OperationContract(IsOneWay = true)]
    void OnLeaderboardChanged(LeaderboardNotification notification);
}
