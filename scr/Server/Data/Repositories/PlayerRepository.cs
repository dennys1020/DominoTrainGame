using System.Linq;

namespace DominoTrainGame;

public sealed class PlayerRepository
{
    public bool Exists(string userName, string email)
    {
        using (DominoGameDBEntities databaseContext = DatabaseContextFactory.Create())
        {
            bool alreadyExists = databaseContext.Players.Any(
                player => player.userName == userName || player.Email == email);

            return alreadyExists;
        }
    }

    public void Register(Player player)
    {
        using (DominoGameDBEntities databaseContext = DatabaseContextFactory.Create())
        {
            databaseContext.Players.Add(player);
            databaseContext.SaveChanges();
        }
    }
}
