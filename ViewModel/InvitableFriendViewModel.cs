using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;

namespace DominoTrainGame.Client.ViewModel;

public sealed class InvitableFriendViewModel : ObservableViewModel
{
    private bool _isInvited;

    public InvitableFriendViewModel(FriendResult friend, Func<InvitableFriendViewModel, Task> invite)
    {
        if (friend == null)
        {
            throw new ArgumentNullException(nameof(friend));
        }

        if (invite == null)
        {
            throw new ArgumentNullException(nameof(invite));
        }

        Username = friend.Username;
        InviteCommand = new AsyncRelayCommand(() => invite(this));
    }

    public string Username { get; }

    public bool IsInvited
    {
        get
        {
            return _isInvited;
        }
        private set
        {
            SetProperty(ref _isInvited, value);
        }
    }

    public AsyncRelayCommand InviteCommand { get; }

    public void MarkInvited()
    {
        IsInvited = true;
    }
}
