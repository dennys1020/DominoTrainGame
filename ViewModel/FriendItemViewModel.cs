using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;

namespace DominoTrainGame.Client.ViewModel;

public sealed class FriendItemViewModel
{
    public FriendItemViewModel(FriendResult friend, string statusLabel, Func<FriendResult, Task> removeFriend)
    {
        if (friend == null)
        {
            throw new ArgumentNullException(nameof(friend));
        }

        if (removeFriend == null)
        {
            throw new ArgumentNullException(nameof(removeFriend));
        }

        PlayerId = friend.PlayerId;
        Username = friend.Username;
        StatusLabel = statusLabel ?? string.Empty;
        IsConnected = friend.Status != FriendStatus.Offline;
        RemoveCommand = new AsyncRelayCommand(() => removeFriend(friend));
    }

    public int PlayerId { get; }

    public string Username { get; }

    public string StatusLabel { get; }

    public bool IsConnected { get; }

    public AsyncRelayCommand RemoveCommand { get; }
}
