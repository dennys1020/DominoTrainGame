using System;
using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel;

public sealed class RoomPlayerViewModel
{
    public RoomPlayerViewModel(LobbyPlayerResult player, int currentPlayerId, int hostPlayerId)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        Username = player.Username;
        IsReady = player.IsReady;
        IsCurrentPlayer = player.PlayerId == currentPlayerId;
        IsHost = player.PlayerId == hostPlayerId;
    }

    public string Username { get; }

    public bool IsCurrentPlayer { get; }

    public bool IsReady { get; }

    public bool IsHost { get; }
}
