using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Persistence.Contracts;

public interface IPlayerRepository
{
    bool TryFindCredentials(string identifier, out PlayerCredentials credentials);
}
