using System.Data.Entity;

namespace DominoTrainGame.Server.Persistence.Data;

[DbConfigurationType(typeof(SqlServerConfiguration))]
public partial class DominoGameDBEntities
{
    public DominoGameDBEntities(string nameOrConnectionString)
        : base(nameOrConnectionString)
    {
    }
}
