namespace DominoTrainGame.Server.Persistence.Models;

public sealed class AccountRecord
{
    public int PlayerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}
