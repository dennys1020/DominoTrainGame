namespace DominoTrainGame.Server.Persistence.Models;

public sealed class PlayerRegistration
{
    public PlayerRegistration(string username, string email, string passwordHash)
    {
        Username = username ?? string.Empty;
        Email = email ?? string.Empty;
        PasswordHash = passwordHash ?? string.Empty;
    }

    public string Username { get; }

    public string Email { get; }

    public string PasswordHash { get; }
}
