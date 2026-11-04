using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(
    CallbackContract = typeof(IGameClientCallback),
    SessionMode = SessionMode.Required)]
public interface ILobbyService
{
    [OperationContract(IsInitiating = true)]
    [FaultContract(typeof(GameServiceFault))]
    void Connect(ConnectRequest request);

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    LobbyResponse CreateLobby();

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    LobbyResponse JoinLobby(JoinLobbyRequest request);

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void LeaveLobby();

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void SetReady(SetReadyRequest request);
}
