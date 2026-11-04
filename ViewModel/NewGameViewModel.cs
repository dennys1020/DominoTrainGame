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
    private readonly LocalizationViewModel _localization;

    public NewGameViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        LocalizationViewModel localization) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        BackCommand = new RelayCommand(() => WindowService.Navigate(Screen.Main));
        JoinGameCommand = new RelayCommand(() => WindowService.Navigate(Screen.JoinGame));
        RoomCommand = new AsyncRelayCommand(CreateLobbyAsync);
    }

    public RelayCommand BackCommand { get; }

    public RelayCommand JoinGameCommand { get; }

    public AsyncRelayCommand RoomCommand { get; }

    private async Task CreateLobbyAsync()
    {
        LobbyResult result = await _gameServices.CreateLobbyAsync();

        if (result.Status == LobbyResultStatus.Success)
        {
            WindowService.Navigate(Screen.Room);
        }
        else
        {
            WindowService.ShowMessage(_localization[GetMessageKey(result.Status)]);
        }
    }

    private static string GetMessageKey(LobbyResultStatus status)
    {
        string messageKey;

        switch (status)
        {
            case LobbyResultStatus.SessionNotFound:
                messageKey = "LobbySessionExpiredMessage";
                break;
            case LobbyResultStatus.ServerUnavailable:
                messageKey = "ServerUnavailableMessage";
                break;
            case LobbyResultStatus.AlreadyInLobby:
                messageKey = "LobbyConflictMessage";
                break;
            default:
                messageKey = "LobbyServerErrorMessage";
                break;
        }

        return messageKey;
    }
}
