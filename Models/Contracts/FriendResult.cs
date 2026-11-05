namespace DominoTrainGame.Client.Models.Contracts;

public sealed class FriendResult
{
    public FriendResult(int playerId, string username, FriendStatus status)
    {
        PlayerId = playerId;
        Username = username ?? string.Empty;
        Status = status;
    }

    public int PlayerId { get; }

    public string Username { get; }

    public FriendStatus Status { get; }
}
