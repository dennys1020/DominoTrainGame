using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;

namespace DominoTrainGame.Client.ViewModel;

public sealed class PendingFriendshipItemViewModel
{
    public PendingFriendshipItemViewModel(
        PendingFriendshipResult request,
        Func<PendingFriendshipResult, Task> acceptRequest,
        Func<PendingFriendshipResult, Task> rejectRequest)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (acceptRequest == null)
        {
            throw new ArgumentNullException(nameof(acceptRequest));
        }

        if (rejectRequest == null)
        {
            throw new ArgumentNullException(nameof(rejectRequest));
        }

        Username = request.RequesterUsername;
        AcceptCommand = new AsyncRelayCommand(() => acceptRequest(request));
        RejectCommand = new AsyncRelayCommand(() => rejectRequest(request));
    }

    public string Username { get; }

    public AsyncRelayCommand AcceptCommand { get; }

    public AsyncRelayCommand RejectCommand { get; }
}
