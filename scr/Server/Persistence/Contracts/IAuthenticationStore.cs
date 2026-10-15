using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Persistence.Contracts;

public interface IAuthenticationStore
{
    AccountRecord FindAccount(string identifier);
    AccountRecord FindAccount(int playerId);
    bool PlayerExists(string username, string email);
    bool SaveNewPlayer(string username, string email, string passwordHash, string preferredLanguage);
    bool SaveNewPassword(int playerId, string passwordHash);
}
