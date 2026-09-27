using System.Linq;

namespace DominoTrainGame;

public sealed class PlayerRepository
{
    public bool Exists(string userName, string email)
    {
        using (DominoGameDBEntities databaseContext = new DominoGameDBEntities())
        {
            bool alreadyExists = databaseContext.Players.Any(
                p => p.userName == userName || p.Email == email);

            return alreadyExists;
        }
    }

    public void Register(Player player)
    {
        using (DominoGameDBEntities dbContext = new DominoGameDBEntities())
        {
            dbContext.Players.Add(player);
            dbContext.SaveChanges();
        }
    }
}