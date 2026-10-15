using System;
using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(CallbackContract = typeof(IAuthenticationCallback))]
public interface IAuthenticationService
{
    [OperationContract(IsOneWay = true)]
    void LogIn(Guid requestId, string identifier, string password);

    [OperationContract(IsOneWay = true)]
    void RegisterUser(Guid requestId, User user);

    [OperationContract(IsOneWay = true)]
    void CheckAvailability(Guid requestId, User user);
}
