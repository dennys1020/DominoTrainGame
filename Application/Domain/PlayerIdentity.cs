namespace DominoTrainGame.Server.Application.Domain;

public sealed class PlayerIdentity
{
    private const int EmptyPlayerId = 0;

    static PlayerIdentity()
    {
        Empty = new PlayerIdentity(EmptyPlayerId, string.Empty);
    }

    public PlayerIdentity(int playerId, string username)
    {
        PlayerId = playerId;
        Username = username ?? string.Empty;
    }

    public static PlayerIdentity Empty { get; }

    public int PlayerId { get; }

    public string Username { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);
}
