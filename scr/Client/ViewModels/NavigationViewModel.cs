namespace DominoTrainGame.ViewModels;

public sealed class NavigationViewModel : ScreenViewModel
{
    public NavigationViewModel(IWindowService window, Screen screen) : base(window)
    {
        NewGameCommand = new RelayCommand(() => Window.Navigate(Screen.NewGame));
        LeaderboardCommand = new RelayCommand(() => Window.ShowDialog(Screen.Leaderboard));
        ResultsCommand = new RelayCommand(() => Window.Navigate(Screen.MatchResults));
        ChangePasswordCommand = new RelayCommand(() => Window.ShowDialog(Screen.ChangePassword));
        DeleteAccountCommand = new RelayCommand(() => Window.ShowDialog(Screen.DeleteAccount));
        ProfileCommand = new RelayCommand(() => Window.ShowDialog(Screen.Profile));
        FriendsCommand = new RelayCommand(() => Window.ShowDialog(Screen.Friends));
        LogoutCommand = new RelayCommand(() => Window.Navigate(Screen.Login));
        JoinGameCommand = new RelayCommand(() => Window.Navigate(Screen.JoinGame));
        RoomCommand = new RelayCommand(() => Window.Navigate(Screen.Room, new RoomContext(screen == Screen.NewGame)));
        BackCommand = new RelayCommand(() => Window.Navigate(screen == Screen.NewGame ? Screen.Main : Screen.NewGame));
    }

    public RelayCommand NewGameCommand { get; }
    public RelayCommand LeaderboardCommand { get; }
    public RelayCommand ResultsCommand { get; }
    public RelayCommand ChangePasswordCommand { get; }
    public RelayCommand DeleteAccountCommand { get; }
    public RelayCommand ProfileCommand { get; }
    public RelayCommand FriendsCommand { get; }
    public RelayCommand LogoutCommand { get; }
    public RelayCommand JoinGameCommand { get; }
    public RelayCommand RoomCommand { get; }
    public RelayCommand BackCommand { get; }
}
