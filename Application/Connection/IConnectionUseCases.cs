using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.Connection;

public interface IConnectionUseCases
{
    DatabaseConnectionStatus CheckDatabaseConnection();
}
