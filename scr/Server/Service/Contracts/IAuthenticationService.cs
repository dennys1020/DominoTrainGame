using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract]
public interface IAuthenticationService
{
    [OperationContract]
    bool LogIn(string identificator, string password);

    [OperationContract]
    bool RegisterUser(User user);
}
