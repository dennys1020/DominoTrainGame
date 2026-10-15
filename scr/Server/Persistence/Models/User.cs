namespace DominoTrainGame.Server.Persistence.Models;

public sealed class User
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = string.Empty;
}
