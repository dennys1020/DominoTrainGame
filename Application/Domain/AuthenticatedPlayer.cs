namespace DominoTrainGame.Server.Application.Domain;

public sealed class AuthenticatedPlayer
{
    private const int EmptyPlayerId = 0;

    static AuthenticatedPlayer()
    {
        Empty = new AuthenticatedPlayer(EmptyPlayerId, string.Empty);
    }

    public AuthenticatedPlayer(int playerId, string username)
    {
        PlayerId = playerId;
        Username = username ?? string.Empty;
    }

    public static AuthenticatedPlayer Empty { get; }

    public int PlayerId { get; }

    public string Username { get; }
}
