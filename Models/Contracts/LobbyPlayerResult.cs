using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyPlayerResult
{
    public LobbyPlayerResult(PlayerSummary member, bool isReady)
    {
        Member = member ?? throw new ArgumentNullException(nameof(member));
        IsReady = isReady;
    }

    public PlayerSummary Member { get; }

    public int PlayerId => Member.PlayerId;

    public string Username => Member.Username;

    public bool IsReady { get; }

    public LobbyPlayerResult WithReadyState(bool isReady)
    {
        LobbyPlayerResult player = new LobbyPlayerResult(Member, isReady);
        return player;
    }
}
