using System;
using System.Globalization;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;

namespace DominoTrainGame.Client.ViewModel.Navigation;

public sealed class LobbyInvitationController
{
    private const string InvitationReceivedMessageKey = "LobbyInvitationReceivedMessage";

    private readonly IRemoteGameServices _gameServices;
    private readonly LocalizationViewModel _localization;
    private IWindowService _windowService;

    public LobbyInvitationController(IRemoteGameServices gameServices, LocalizationViewModel localization)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _gameServices.LobbyEvents.LobbyInvitationReceived += HandleLobbyInvitationReceived;
    }

    public void Attach(IWindowService windowService)
    {
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
    }

    private void HandleLobbyInvitationReceived(object sender, LobbyInvitationReceivedEventArgs eventArgs)
    {
        IWindowService windowService = _windowService;
        bool hasWindow = windowService != null;
        bool isInLobby = _gameServices.CurrentLobby.Status == LobbyResultStatus.Success;

        if (hasWindow && !isInLobby)
        {
            AsyncRelayCommand answerCommand = new AsyncRelayCommand(
                () => AnswerInvitationAsync(windowService, eventArgs));
            answerCommand.Execute(null);
        }
    }

    private async Task AnswerInvitationAsync(IWindowService windowService, LobbyInvitationReceivedEventArgs invitation)
    {
        string message = string.Format(
            CultureInfo.CurrentCulture,
            _localization[InvitationReceivedMessageKey],
            invitation.InviterUsername);
        bool isAccepted = windowService.Confirm(message);

        if (isAccepted)
        {
            await JoinInvitedLobbyAsync(windowService, invitation.Code);
        }
    }

    private async Task JoinInvitedLobbyAsync(IWindowService windowService, string code)
    {
        LobbyResult result = await _gameServices.JoinLobbyAsync(code);

        if (result.Status == LobbyResultStatus.Success)
        {
            windowService.Navigate(Screen.Room);
        }
        else
        {
            windowService.ShowMessage(LobbyResultMessageKeys.GetMessageKey(result.Status));
        }
    }
}
