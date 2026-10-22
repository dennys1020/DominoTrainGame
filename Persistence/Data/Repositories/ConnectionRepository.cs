using System;
using System.Data;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Persistence.Data.Repositories;

public sealed class ConnectionRepository : IConnectionRepository
{
    private readonly DatabaseExecutor _databaseExecutor;

    public ConnectionRepository(DatabaseExecutor databaseExecutor)
    {
        _databaseExecutor = databaseExecutor ?? throw new ArgumentNullException(nameof(databaseExecutor));
    }

    public bool CanConnect()
    {
        bool canConnect = _databaseExecutor.Execute(OpenConnection);
        return canConnect;
    }

    private static bool OpenConnection(DominoGameDBEntities context)
    {
        context.Database.Connection.Open();
        bool isOpen = context.Database.Connection.State == ConnectionState.Open;
        return isOpen;
    }
}
