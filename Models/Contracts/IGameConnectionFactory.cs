using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IGameConnectionFactory
{
    ConnectionServiceClient CreateConnectionClient();

    AuthenticationServiceClient CreateAuthenticationClient();
}
