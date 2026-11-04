using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyPlayerResult
{
    public LobbyPlayerResult(LobbyMember member, bool isHost, bool isReady)
    {
        Member = member ?? throw new ArgumentNullException(nameof(member));
        IsHost = isHost;
        IsReady = isReady;
    }

    public LobbyMember Member { get; }

    public int PlayerId => Member.PlayerId;

    public string Username => Member.Username;

    public bool IsHost { get; }

    public bool IsReady { get; }

    public LobbyPlayerResult WithHostState(bool isHost)
    {
        LobbyPlayerResult player = new LobbyPlayerResult(Member, isHost, IsReady);
        return player;
    }

    public LobbyPlayerResult WithReadyState(bool isReady)
    {
        LobbyPlayerResult player = new LobbyPlayerResult(Member, IsHost, isReady);
        return player;
    }
}
