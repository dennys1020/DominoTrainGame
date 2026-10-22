using DominoTrainGame.Server.Persistence.Models;

public interface IPlayerRepository
{
    bool TryFindCredentials(string identifier, out PlayerCredentials credentials);

    bool TryCreatePlayer(PlayerRegistration registration, out PlayerCredentials credentials);

    bool IsUsernameTaken(string username);
    bool IsEmailTaken(string email);
    void AddPlayer(string username, string email, string passwordHash);
}
