using System;
using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

public interface ILeaderboardCallback
{
    [OperationContract(IsOneWay = true)]
    void OnLeaderboardReceived(Guid requestId, LeaderboardResponse response);
}
