using System.Data.Entity;

namespace DominoTrainGame;

[DbConfigurationType(typeof(SqlServerConfiguration))]
public partial class DominoGameDBEntities
{
    public DominoGameDBEntities(string nameOrConnectionString)
        : base(nameOrConnectionString)
    {
    }
}
