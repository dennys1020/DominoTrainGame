namespace DominoTrainGame.Server.Persistence.Models;

public sealed class PlayerCredentials
{
    private const int EmptyPlayerId = 0;

    static PlayerCredentials()
    {
        Empty = new PlayerCredentials(EmptyPlayerId, string.Empty, string.Empty);
    }

    public PlayerCredentials(int playerId, string username, string passwordHash)
    {
        PlayerId = playerId;
        Username = username ?? string.Empty;
        PasswordHash = passwordHash ?? string.Empty;
    }

    public static PlayerCredentials Empty { get; }

    public int PlayerId { get; }

    public string Username { get; }

    public string PasswordHash { get; }
}
