namespace DominoTrainGame.Client.Models.Contracts;

public sealed class FoundPlayerResult
{
    public FoundPlayerResult(int playerId, string username)
    {
        PlayerId = playerId;
        Username = username ?? string.Empty;
    }

    public int PlayerId { get; }

    public string Username { get; }
}
