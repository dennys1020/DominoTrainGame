using System.ServiceModel;
using System.Threading.Tasks;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(Namespace = "urn:domino-train:services:v1")]
public interface IVerificationService
{
    [OperationContract]
    Task<VerificationSessionDto> BeginVerificationAsync(BeginVerificationRequest request);

    [OperationContract]
    Task<VerificationSessionDto> ResendVerificationAsync(ResendVerificationRequest request);

    [OperationContract]
    VerificationCodeResponse VerifyCode(VerifyCodeRequest request);
}
