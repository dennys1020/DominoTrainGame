namespace DominoTrainGame.Client.Models.Contracts;

public sealed class PlayerSummary
{
    public const int EmptyPlayerId = 0;

    public PlayerSummary(int playerId, string username)
    {
        PlayerId = playerId;
        Username = username ?? string.Empty;
    }

    public int PlayerId { get; }

    public string Username { get; }
}
