using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;

namespace DominoTrainGame.Client.ViewModel;

public sealed class PlayerSearchItemViewModel
{
    public PlayerSearchItemViewModel(FoundPlayerResult player, Func<FoundPlayerResult, Task> sendRequest)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (sendRequest == null)
        {
            throw new ArgumentNullException(nameof(sendRequest));
        }

        Username = player.Username;
        SendRequestCommand = new AsyncRelayCommand(() => sendRequest(player));
    }

    public string Username { get; }

    public AsyncRelayCommand SendRequestCommand { get; }
}
