using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class NewGameViewModel : ScreenViewModel
{
    private readonly IRemoteGameServices _gameServices;

    public NewGameViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        BackCommand = new RelayCommand(() => WindowService.Navigate(Screen.Main));
        JoinGameCommand = new RelayCommand(() => WindowService.Navigate(Screen.JoinGame));
        RoomCommand = new AsyncRelayCommand(CreateLobbyAsync);
    }

    public RelayCommand BackCommand { get; }

    public RelayCommand JoinGameCommand { get; }

    public AsyncRelayCommand RoomCommand { get; }

    private async Task CreateLobbyAsync()
    {
        LobbyResult result = await RunWhileBusyAsync(() => _gameServices.CreateLobbyAsync());

        if (result.Status == LobbyResultStatus.Success)
        {
            WindowService.Navigate(Screen.Room);
        }
        else
        {
            WindowService.ShowMessage(LobbyResultMessageKeys.GetMessageKey(result.Status));
        }
    }
}
