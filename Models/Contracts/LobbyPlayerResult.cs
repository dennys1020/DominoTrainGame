using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbyPlayerResult
{
    public LobbyPlayerResult(int playerId, string username, bool isHost)
    {
        PlayerId = playerId;
        Username = username ?? string.Empty;
        IsHost = isHost;
    }

    public int PlayerId { get; }

    public string Username { get; }

    public bool IsHost { get; }
}
