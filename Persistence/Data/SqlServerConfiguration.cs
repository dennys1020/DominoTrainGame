using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace DominoTrainGame.Server.Persistence.Data;

public sealed class SqlServerConfiguration : DbConfiguration
{
    public SqlServerConfiguration()
    {
        SetProviderServices(SqlProviderServices.ProviderInvariantName, SqlProviderServices.Instance);
    }
}
