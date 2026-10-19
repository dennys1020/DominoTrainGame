using DominoTrainGame.ViewModels.Commands;
using DominoTrainGame.ViewModels.Common;
using DominoTrainGame.ViewModels.Navigation;

namespace DominoTrainGame.ViewModels;

public sealed class RoomViewModel : ScreenViewModel
{
    public RoomViewModel(IWindowService windowService, RoomRole roomRole) : base(windowService)
    {
        IsHost = roomRole == RoomRole.Host;
        Screen previousScreen = IsHost ? Screen.NewGame : Screen.JoinGame;

        BackCommand = new RelayCommand(() => WindowService.Navigate(previousScreen));
        ResultsCommand = new RelayCommand(() => WindowService.Navigate(Screen.MatchResults, roomRole));
    }

    public bool IsHost { get; }

    public RelayCommand BackCommand { get; }

    public RelayCommand ResultsCommand { get; }
}
