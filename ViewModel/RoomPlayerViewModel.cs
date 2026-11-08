using System;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Common;

namespace DominoTrainGame.Client.ViewModel;

public sealed class RoomPlayerViewModel : ObservableViewModel
{
    private bool _isHost;
    private bool _isReady;

    public RoomPlayerViewModel(LobbyPlayerResult player, int currentPlayerId)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        PlayerId = player.PlayerId;
        Username = player.Username;
        _isReady = player.IsReady;
        IsCurrentPlayer = player.PlayerId == currentPlayerId;
    }

    public int PlayerId { get; }

    public string Username { get; }

    public bool IsCurrentPlayer { get; }

    public bool IsReady
    {
        get
        {
            return _isReady;
        }
        set
        {
            SetProperty(ref _isReady, value);
        }
    }

    public bool IsHost
    {
        get
        {
            return _isHost;
        }
        set
        {
            SetProperty(ref _isHost, value);
        }
    }
}
