using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Persistence.Contracts;

public interface IAuthenticationStore
{
    AccountRecord FindAccount(string identifier);
    bool PlayerExists(string username, string email);
    bool SaveNewPlayer(string username, string email, string passwordHash, string preferredLanguage);
}
