using System.Data;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Data;

namespace DominoTrainGame.Server.Persistence.Repositories;

public sealed class ConnectionRepository : IConnectionRepository
{
    public bool CanConnect()
    {
        bool canConnect = DatabaseExecutor.Execute(OpenConnection);
        return canConnect;
    }

    private static bool OpenConnection(DominoGameDBEntities context)
    {
        context.Database.Connection.Open();
        bool isOpen = context.Database.Connection.State == ConnectionState.Open;
        return isOpen;
    }
}
