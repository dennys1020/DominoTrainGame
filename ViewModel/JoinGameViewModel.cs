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
    private readonly LocalizationViewModel _localization;
    private string _lobbyCode;

    public JoinGameViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        LocalizationViewModel localization) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
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
        LobbyResult result = await _gameServices.JoinLobbyAsync(LobbyCode.Trim());

        if (result.Status == LobbyResultStatus.Success)
        {
            WindowService.Navigate(Screen.Room, RoomRole.Guest);
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
            case LobbyResultStatus.InvalidCode:
                messageKey = "InvalidLobbyCodeMessage";
                break;
            case LobbyResultStatus.LobbyNotFound:
                messageKey = "LobbyNotFoundMessage";
                break;
            case LobbyResultStatus.LobbyFull:
                messageKey = "LobbyFullMessage";
                break;
            case LobbyResultStatus.AlreadyInLobby:
                messageKey = "LobbyConflictMessage";
                break;
            case LobbyResultStatus.SessionNotFound:
                messageKey = "LobbySessionExpiredMessage";
                break;
            case LobbyResultStatus.ServerUnavailable:
                messageKey = "ServerUnavailableMessage";
                break;
            default:
                messageKey = "LobbyServerErrorMessage";
                break;
        }

        return messageKey;
    }
}
