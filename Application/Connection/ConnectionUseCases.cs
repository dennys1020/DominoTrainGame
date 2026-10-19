using System;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Errors;

namespace DominoTrainGame.Server.Application.Connection;

public sealed class ConnectionUseCases : IConnectionUseCases
{
    private static readonly ITraceLog _logger;
    private readonly IConnectionRepository _connectionRepository;

    static ConnectionUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(ConnectionUseCases));
    }

    public ConnectionUseCases(IConnectionRepository connectionRepository)
    {
        _connectionRepository = connectionRepository
            ?? throw new ArgumentNullException(nameof(connectionRepository));
    }

    public DatabaseConnectionStatus CheckDatabaseConnection()
    {
        DatabaseConnectionStatus connectionStatus = DatabaseConnectionStatus.Unavailable;

        try
        {
            bool canConnect = _connectionRepository.CanConnect();
            connectionStatus = canConnect
                ? DatabaseConnectionStatus.Available
                : DatabaseConnectionStatus.Unavailable;
        }
        catch (DatabaseOperationException exception)
        {
            _logger.Error("The database connection could not be verified.", exception);
        }

        return connectionStatus;
    }
}
