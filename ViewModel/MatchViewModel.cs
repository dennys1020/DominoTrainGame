using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class MatchViewModel : ScreenViewModel
{
    public MatchViewModel(IWindowService windowService) : base(windowService)
    {
        ResultsCommand = new RelayCommand(() => WindowService.Navigate(Screen.MatchResults));
    }

    public RelayCommand ResultsCommand { get; }
}
