using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(Namespace = "urn:domino-train:services:v1", CallbackContract = typeof(IVerificationCallback))]
public interface IVerificationService
{
    [OperationContract(IsOneWay = true)]
    void BeginVerification(BeginVerificationRequest request);

    [OperationContract(IsOneWay = true)]
    void ResendVerification(ResendVerificationRequest request);

    [OperationContract]
    VerificationCodeResponse VerifyCode(VerifyCodeRequest request);
}
