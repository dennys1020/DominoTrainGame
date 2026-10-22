using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

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
