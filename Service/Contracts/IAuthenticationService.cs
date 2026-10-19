using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(CallbackContract = typeof(IAuthenticationCallback))]
public interface IAuthenticationService
{
    [OperationContract(IsOneWay = true)]
    void LogIn(LoginRequest request);
}
