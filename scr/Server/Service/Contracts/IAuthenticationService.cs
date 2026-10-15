using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(Namespace = "urn:domino-train:services:v1")]
public interface IAuthenticationService
{
    [OperationContract]
    LoginResponse LogIn(LoginRequest request);

    [OperationContract]
    SignUpResponse CheckAvailability(RegistrationRequest request);

    [OperationContract]
    SignUpResponse RegisterUser(RegistrationRequest request);

    [OperationContract]
    RecoveryAccountDto FindAccount(FindAccountRequest request);

    [OperationContract]
    PasswordResetResponse UpdatePassword(PasswordResetRequest request);
}
