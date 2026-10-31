namespace DominoTrainGame.Server.Application.Connection;

public sealed class LobbyPlayerState
{
    public LobbyPlayerState(int playerId, string username, bool isHost)
    {
        PlayerId = playerId;
        Username = username ?? string.Empty;
        IsHost = isHost;
    }

    public int PlayerId { get; }

    public string Username { get; }

    public bool IsHost { get; }
}
