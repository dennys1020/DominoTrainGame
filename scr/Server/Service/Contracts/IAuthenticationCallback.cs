using System;
using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

public interface IAuthenticationCallback
{
    [OperationContract(IsOneWay = true)]
    void OnLogInCompleted(Guid requestId, LoginStatus status);

    [OperationContract(IsOneWay = true)]
    void OnRegistrationCompleted(Guid requestId, RegistrationStatus status);

    [OperationContract(IsOneWay = true)]
    void OnAvailabilityChecked(Guid requestId, RegistrationStatus status);
}
