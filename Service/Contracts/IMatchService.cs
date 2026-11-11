using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract]
public interface IMatchService
{
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void PlayTile(PlayTileRequest request);

    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void DrawTile(MatchTurnRequest request);

    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void PassTurn(MatchTurnRequest request);
}
