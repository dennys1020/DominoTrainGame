using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract]
public interface IAuthenticationService
{
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    LoginResponse LogIn(LoginRequest request);

    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    bool SignUp(SignUpRequest request);
}
