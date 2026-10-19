using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IRemoteGameServices
{
    Task<ServerConnectionStatus> CheckConnectionAsync();

    Task<LoginResult> LogInAsync(string identifier, string password);
}
