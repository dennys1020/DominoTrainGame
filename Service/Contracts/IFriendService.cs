using System.Collections.Generic;
using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(
    CallbackContract = typeof(ISocialClientCallback),
    SessionMode = SessionMode.Required)]
public interface IFriendService
{
    [OperationContract(IsInitiating = true)]
    [FaultContract(typeof(GameServiceFault))]
    void Connect(ConnectRequest request);

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    List<FriendResponse> GetFriends();

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    List<PendingFriendRequestResponse> GetPendingRequests();

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    List<PlayerSearchResponse> SearchPlayers(PlayerSearchRequest request);

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void SendFriendRequest(FriendUsernameRequest request);

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void AcceptFriendRequest(FriendshipDecisionRequest request);

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void RejectFriendRequest(FriendshipDecisionRequest request);

    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void RemoveFriend(FriendUsernameRequest request);
}
