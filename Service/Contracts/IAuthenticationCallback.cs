using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract]
public interface IAuthenticationCallback
{
    [OperationContract(IsOneWay = true)]
    void OnLogInCompleted(LoginResponse response);
}
