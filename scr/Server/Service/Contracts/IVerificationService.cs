using System;
using System.ServiceModel;
using System.Threading.Tasks;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(CallbackContract = typeof(IVerificationCallback))]
public interface IVerificationService
{
    [OperationContract(IsOneWay = true)]
    Task BeginVerificationAsync(Guid requestId, BeginVerificationRequest request);

    [OperationContract(IsOneWay = true)]
    Task ResendVerificationAsync(Guid requestId, Guid sessionId);

    [OperationContract(IsOneWay = true)]
    void VerifyCode(Guid requestId, Guid sessionId, string code);
}
