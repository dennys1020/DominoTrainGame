using DominoTrainGame.ViewModels.Commands;
using DominoTrainGame.ViewModels.Common;
using DominoTrainGame.ViewModels.Navigation;

namespace DominoTrainGame.ViewModels;

public sealed class NewGameViewModel : ScreenViewModel
{
    public NewGameViewModel(IWindowService windowService) : base(windowService)
    {
        BackCommand = new RelayCommand(() => WindowService.Navigate(Screen.Main));
        JoinGameCommand = new RelayCommand(() => WindowService.Navigate(Screen.JoinGame));
        RoomCommand = new RelayCommand(() => WindowService.Navigate(Screen.Room, RoomRole.Host));
    }

    public RelayCommand BackCommand { get; }

    public RelayCommand JoinGameCommand { get; }

    public RelayCommand RoomCommand { get; }
}
