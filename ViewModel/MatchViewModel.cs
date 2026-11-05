using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class MatchViewModel : ScreenViewModel
{
    public MatchViewModel(IWindowService windowService, RoomRole roomRole) : base(windowService)
    {
        ResultsCommand = new RelayCommand(() => WindowService.Navigate(Screen.MatchResults, roomRole));
    }

    public RelayCommand ResultsCommand { get; }
}
