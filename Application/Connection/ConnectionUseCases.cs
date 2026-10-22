using System;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.Connection;

public sealed class ConnectionUseCases : IConnectionUseCases
{
    private readonly IConnectionRepository _connectionRepository;

    public ConnectionUseCases(IConnectionRepository connectionRepository)
    {
        _connectionRepository = connectionRepository
            ?? throw new ArgumentNullException(nameof(connectionRepository));
    }

    public void VerifyDatabaseConnection()
    {
        bool canConnect = _connectionRepository.CanConnect();

        if (!canConnect)
        {
            throw new DatabaseUnavailableException("The database connection could not be opened.");
        }
    }
}
