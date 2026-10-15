using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(Namespace = "urn:domino-train:services:v1", CallbackContract = typeof(ILeaderboardCallback))]
public interface ILeaderboardService
{
    [OperationContract(IsOneWay = true)]
    Task GetLeaderboardAsync(Guid requestId);
}
