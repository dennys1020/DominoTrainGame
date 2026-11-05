namespace DominoTrainGame.Server.Application.Domain;

public sealed class LobbyMember
{
    public LobbyMember(int playerId, string username)
    {
        PlayerId = playerId;
        Username = username ?? string.Empty;
    }

    public int PlayerId { get; }

    public string Username { get; }
}
