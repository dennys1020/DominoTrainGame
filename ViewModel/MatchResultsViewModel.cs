using DominoTrainGame.ViewModels.Commands;
using DominoTrainGame.ViewModels.Common;
using DominoTrainGame.ViewModels.Navigation;

namespace DominoTrainGame.ViewModels;

public sealed class MatchResultsViewModel : ScreenViewModel
{
    public MatchResultsViewModel(IWindowService windowService, RoomRole roomRole) : base(windowService)
    {
        bool isHost = roomRole == RoomRole.Host;
        Screen previousScreen = isHost ? Screen.NewGame : Screen.JoinGame;

        BackCommand = new RelayCommand(() => WindowService.Navigate(previousScreen));
        MainMenuCommand = new RelayCommand(() => WindowService.Navigate(Screen.Main));
        RoomCommand = new RelayCommand(() => WindowService.Navigate(Screen.Room, roomRole));
    }

    public RelayCommand BackCommand { get; }

    public RelayCommand MainMenuCommand { get; }

    public RelayCommand RoomCommand { get; }
}
