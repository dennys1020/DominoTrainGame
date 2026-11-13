using System;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class PlayerSanction
{
    static PlayerSanction()
    {
        Empty = new PlayerSanction(SanctionType.ChatMute, DateTime.MinValue);
    }

    public PlayerSanction(SanctionType type, DateTime expiresAt)
    {
        Type = type;
        ExpiresAt = expiresAt;
    }

    public static PlayerSanction Empty { get; }

    public SanctionType Type { get; }

    public DateTime ExpiresAt { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);
}
