using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;

namespace DominoTrainGame.Client.ViewModel;

public sealed class FriendItemViewModel
{
    public FriendItemViewModel(FriendResult friend, Func<FriendResult, Task> removeFriend)
    {
        if (friend == null)
        {
            throw new ArgumentNullException(nameof(friend));
        }

        if (removeFriend == null)
        {
            throw new ArgumentNullException(nameof(removeFriend));
        }

        Username = friend.Username;
        RemoveCommand = new AsyncRelayCommand(() => removeFriend(friend));
    }

    public string Username { get; }

    public AsyncRelayCommand RemoveCommand { get; }
}
