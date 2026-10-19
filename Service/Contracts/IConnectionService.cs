using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(Name = "IConnectionService", Namespace = "http://dominotraingame/services")]
public interface IConnectionService
{
    [OperationContract]
    ServerConnectionStatus CheckConnection();
}
