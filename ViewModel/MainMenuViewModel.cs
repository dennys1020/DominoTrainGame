using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class MainMenuViewModel : ScreenViewModel
{
    public MainMenuViewModel(IWindowService windowService) : base(windowService)
    {
        NewGameCommand = new RelayCommand(() => WindowService.Navigate(Screen.NewGame));
        ResultsCommand = new RelayCommand(() => WindowService.Navigate(Screen.MatchResults, RoomRole.Host));
        LogoutCommand = new RelayCommand(() => WindowService.Navigate(Screen.Login));
        LeaderboardCommand = new RelayCommand(() => WindowService.ShowDialog(Screen.Leaderboard));
        ProfileCommand = new RelayCommand(() => WindowService.ShowDialog(Screen.Profile));
        FriendsCommand = new RelayCommand(() => WindowService.ShowDialog(Screen.Friends));
    }

    public RelayCommand NewGameCommand { get; }

    public RelayCommand ResultsCommand { get; }

    public RelayCommand LogoutCommand { get; }

    public RelayCommand LeaderboardCommand { get; }

    public RelayCommand ProfileCommand { get; }

    public RelayCommand FriendsCommand { get; }
}
