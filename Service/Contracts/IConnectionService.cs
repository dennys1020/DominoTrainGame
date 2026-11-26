using System.ServiceModel;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Contracts;

/// <summary>
/// Lets the client check that the server and the database are available before the login.
/// </summary>
[ServiceContract]
public interface IConnectionService
{
    /// <summary>
    /// Checks that the server can open a connection to the database.
    /// </summary>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void CheckConnection();
}
