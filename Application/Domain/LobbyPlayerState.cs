using System;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class LobbyPlayerState
{
    private const bool EmptyReadyState = false;

    static LobbyPlayerState()
    {
        Empty = new LobbyPlayerState(PlayerIdentity.Empty, EmptyReadyState);
    }

    public LobbyPlayerState(PlayerIdentity member, bool isReady)
    {
        Member = member ?? throw new ArgumentNullException(nameof(member));
        IsReady = isReady;
    }

    public static LobbyPlayerState Empty { get; }

    public PlayerIdentity Member { get; }

    public int PlayerId => Member.PlayerId;

    public string Username => Member.Username;

    public bool IsReady { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);

    public LobbyPlayerState WithReadyState(bool isReady)
    {
        LobbyPlayerState player = new LobbyPlayerState(Member, isReady);
        return player;
    }
}
