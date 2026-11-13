using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(
    CallbackContract = typeof(IGameClientCallback),
    SessionMode = SessionMode.Required)]
public interface ILobbyService
{
    [OperationContract(IsInitiating = true)]
    [FaultContract(typeof(GameServiceFault))]
    void Connect(SessionTokenRequest request);

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

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void SendLobbyMessage(SendLobbyMessageRequest request);

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void StartMatch();

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void InviteFriend(FriendUsernameRequest request);
}
