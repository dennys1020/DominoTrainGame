using System;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Implementation;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class ConnectionService : IConnectionService
{
    private static readonly ITraceLog _logger;

    private readonly IConnectionUseCases _connectionUseCases;

    static ConnectionService()
    {
        _logger = TraceLogManager.GetLogger(typeof(ConnectionService));
    }

    public ConnectionService(IConnectionUseCases connectionUseCases)
    {
        _connectionUseCases = connectionUseCases ?? throw new ArgumentNullException(nameof(connectionUseCases));
    }

    public void CheckConnection()
    {
        try
        {
            _connectionUseCases.VerifyDatabaseConnection();
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("A connection check failed because the database is unavailable.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.DatabaseUnavailable);
        }
    }
}
