using System;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;

namespace DominoTrainGame.Client.ViewModel.Navigation;

public sealed class ConnectionLossController
{
    private IWindowService _windowService;

    public ConnectionLossController(IRemoteGameServices gameServices)
    {
        if (gameServices == null)
        {
            throw new ArgumentNullException(nameof(gameServices));
        }

        gameServices.LobbyEvents.ConnectionStateChanged += HandleConnectionStateChanged;
    }

    public void Attach(IWindowService windowService)
    {
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
    }

    private static string GetEndedConnectionMessageKey(ConnectionState state)
    {
        string messageKey = state == ConnectionState.Expired
            ? CommonMessageKeys.SessionExpiredMessageKey
            : CommonMessageKeys.ServerUnavailableMessageKey;
        return messageKey;
    }

    private void HandleConnectionStateChanged(object sender, ConnectionStateChangedEventArgs eventArgs)
    {
        IWindowService windowService = _windowService;
        bool hasWindow = windowService != null;
        bool hasConnectionEnded = eventArgs.State == ConnectionState.Lost
            || eventArgs.State == ConnectionState.Expired;

        if (hasWindow && hasConnectionEnded)
        {
            windowService.ShowMessageError(GetEndedConnectionMessageKey(eventArgs.State));
            windowService.Navigate(Screen.Login);
        }
    }
}
