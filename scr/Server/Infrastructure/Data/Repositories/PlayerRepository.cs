using System.Linq;

namespace DominoTrainGame;

public sealed class PlayerRepository
{
    public bool Exists(string username, string email)
    {
        using (DominoGameDBEntities databaseContext = DatabaseContextFactory.Create())
        {
            bool alreadyExists = databaseContext.Players.Any(
                player => player.Username == username || player.Email == email);

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
