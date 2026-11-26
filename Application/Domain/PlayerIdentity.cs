namespace DominoTrainGame.Server.Application.Domain;

public sealed class PlayerIdentity
{
    public const int EmptyPlayerId = 0;

    static PlayerIdentity()
    {
        Empty = new PlayerIdentity(EmptyPlayerId, string.Empty);
    }

    public PlayerIdentity(int playerId, string username)
        : this(playerId, username, false)
    {
    }

    private PlayerIdentity(int playerId, string username, bool isGuest)
    {
        PlayerId = playerId;
        Username = username ?? string.Empty;
        IsGuest = isGuest;
    }

    public static PlayerIdentity Empty { get; }

    public int PlayerId { get; }

    public string Username { get; }

    public bool IsGuest { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);

    public static PlayerIdentity CreateGuest(int playerId, string username)
    {
        PlayerIdentity guest = new PlayerIdentity(playerId, username, true);
        return guest;
    }
}
