using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(CallbackContract = typeof(IAuthenticationCallback))]
public interface IAuthenticationService
{
    [OperationContract(IsOneWay = true)]
    bool LogIn(LoginRequest request);

    [OperationContract(isOneWay = true)]

    bool SignUp(SignUpRequest request);

}
