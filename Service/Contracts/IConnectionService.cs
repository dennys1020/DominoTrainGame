using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract]
public interface IConnectionService
{
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void CheckConnection();
}
