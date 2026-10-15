using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(Namespace = "urn:domino-train:services:v1", CallbackContract = typeof(ILeaderboardCallback))]
public interface ILeaderboardService
{
    [OperationContract(IsOneWay = true)]
    void GetLeaderboard(LeaderboardRequest request);
}
