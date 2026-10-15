using System;
using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

public interface IVerificationCallback
{
    [OperationContract(IsOneWay = true)]
    void OnVerificationStarted(Guid requestId, VerificationSessionDto session);

    [OperationContract(IsOneWay = true)]
    void OnVerificationResent(Guid requestId, bool wasSent);

    [OperationContract(IsOneWay = true)]
    void OnCodeVerified(Guid requestId, VerificationStatus status);
}
