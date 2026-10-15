using System.ServiceModel;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

public interface IVerificationCallback
{
    [OperationContract(IsOneWay = true)]
    void OnVerificationChanged(VerificationNotification notification);
}
