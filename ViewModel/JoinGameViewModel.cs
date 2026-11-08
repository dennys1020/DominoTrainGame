using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class JoinGameViewModel : ScreenViewModel
{
    private readonly IRemoteGameServices _gameServices;
    private string _lobbyCode;

    public JoinGameViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _lobbyCode = string.Empty;
        BackCommand = new RelayCommand(() => WindowService.Navigate(Screen.NewGame));
        RoomCommand = new AsyncRelayCommand(JoinLobbyAsync);
    }

    public string LobbyCode
    {
        get
        {
            return _lobbyCode;
        }
        set
        {
            SetProperty(ref _lobbyCode, value ?? string.Empty);
        }
    }

    public RelayCommand BackCommand { get; }

    public AsyncRelayCommand RoomCommand { get; }

    private async Task JoinLobbyAsync()
    {
        LobbyResult result = await RunWhileBusyAsync(() => _gameServices.JoinLobbyAsync(LobbyCode.Trim()));

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
