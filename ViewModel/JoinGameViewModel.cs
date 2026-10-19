using DominoTrainGame.ViewModels.Commands;
using DominoTrainGame.ViewModels.Common;
using DominoTrainGame.ViewModels.Navigation;

namespace DominoTrainGame.ViewModels;

public sealed class JoinGameViewModel : ScreenViewModel
{
    public JoinGameViewModel(IWindowService windowService) : base(windowService)
    {
        BackCommand = new RelayCommand(() => WindowService.Navigate(Screen.NewGame));
        RoomCommand = new RelayCommand(() => WindowService.Navigate(Screen.Room, RoomRole.Guest));
    }

    public RelayCommand BackCommand { get; }

    public RelayCommand RoomCommand { get; }
}
