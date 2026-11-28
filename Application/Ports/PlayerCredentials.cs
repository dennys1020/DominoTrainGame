using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class PlayerCredentials
{
    static PlayerCredentials()
    {
        Empty = new PlayerCredentials(StateIdentifiers.EmptyPlayerId, string.Empty, string.Empty);
    }

    public PlayerCredentials(int playerId, string username, string passwordHash)
    {
        PlayerId = playerId;
        Username = username ?? string.Empty;
        PasswordHash = passwordHash ?? string.Empty;
    }

    public static PlayerCredentials Empty { get; }

    public int PlayerId { get; }

    public string Username { get; }

    public string PasswordHash { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);
}
