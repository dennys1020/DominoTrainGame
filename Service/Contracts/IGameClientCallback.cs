using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(
    Name = "IGameClientCallback",
    Namespace = "http://tempuri.org/")]
public interface IGameClientCallback
{
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceivePlayerJoined",
        IsOneWay = true)]
    void ReceivePlayerJoined(PlayerJoinedMessage message);

    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceivePlayerLeft",
        IsOneWay = true)]
    void ReceivePlayerLeft(PlayerLeftMessage message);

    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceivePlayerReadyChanged",
        IsOneWay = true)]
    void ReceivePlayerReadyChanged(PlayerReadyChangedMessage message);
}
