using System;

namespace DominoTrainGame.Server.Application.Connection;

public sealed class LobbyPlayerState
{
    private const int EmptyPlayerId = 0;
    private const bool EmptyHostState = false;
    private const bool EmptyReadyState = false;

    static LobbyPlayerState()
    {
        LobbyMember emptyMember = new LobbyMember(EmptyPlayerId, string.Empty);
        Empty = new LobbyPlayerState(emptyMember, EmptyHostState, EmptyReadyState);
    }

    public LobbyPlayerState(LobbyMember member, bool isHost, bool isReady)
    {
        Member = member ?? throw new ArgumentNullException(nameof(member));
        IsHost = isHost;
        IsReady = isReady;
    }

    public static LobbyPlayerState Empty { get; }

    public LobbyMember Member { get; }

    public int PlayerId => Member.PlayerId;

    public string Username => Member.Username;

    public bool IsHost { get; }

    public bool IsReady { get; }

    public LobbyPlayerState WithHostState(bool isHost)
    {
        LobbyPlayerState player = new LobbyPlayerState(Member, isHost, IsReady);
        return player;
    }

    public LobbyPlayerState WithReadyState(bool isReady)
    {
        LobbyPlayerState player = new LobbyPlayerState(Member, IsHost, isReady);
        return player;
    }
}
