using System.ServiceModel;
using System.Threading.Tasks;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Contracts;

[ServiceContract(Namespace = "urn:domino-train:services:v1")]
public interface ILeaderboardService
{
    [OperationContract]
    Task<LeaderboardResponse> GetLeaderboardAsync();
}
