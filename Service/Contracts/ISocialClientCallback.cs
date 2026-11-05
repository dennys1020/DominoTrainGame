using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract]
public interface ISocialClientCallback
{
    [OperationContract(IsOneWay = true)]
    void ReceiveFriendStatusChanged(FriendStatusMessage message);

    [OperationContract(IsOneWay = true)]
    void ReceiveFriendsChanged();
}
