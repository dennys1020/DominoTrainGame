using System;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class MatchResultsViewModel : ScreenViewModel
{
    public MatchResultsViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices) : base(windowService)
    {
        if (gameServices == null)
        {
            throw new ArgumentNullException(nameof(gameServices));
        }

        bool isHost = gameServices.CurrentLobby.HostPlayerId == gameServices.CurrentPlayerId;
        Screen previousScreen = isHost ? Screen.NewGame : Screen.JoinGame;

        BackCommand = new RelayCommand(() => WindowService.Navigate(previousScreen));
        MainMenuCommand = new RelayCommand(() => WindowService.Navigate(Screen.Main));
        RoomCommand = new RelayCommand(() => WindowService.Navigate(Screen.Room));
    }

    public RelayCommand BackCommand { get; }

    public RelayCommand MainMenuCommand { get; }

    public RelayCommand RoomCommand { get; }
}
