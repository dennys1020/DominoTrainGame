namespace DominoTrainGame.ViewModels;

public sealed class NavigationViewModel : ScreenViewModel
{
    private bool _isFriendsSelected = true;

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
        RoomCommand = new RelayCommand(() => Window.Navigate(Screen.Room));
        MainMenuCommand = new RelayCommand(() => Window.Navigate(Screen.Main));
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
    public RelayCommand MainMenuCommand { get; }

    // These bindings keep the remaining windows available as visual previews.
    public RoomViewModel Room { get; } = new RoomViewModel();
    public MatchResultsViewModel Results { get; } = new MatchResultsViewModel();
    public bool IsHost => false;
    public bool IsIdentifyStep => true;
    public bool IsPasswordStep => false;
    public string Identifier { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string Confirmation { get; set; } = string.Empty;
    public string Message => string.Empty;
    public bool IsFriendsSelected
    {
        get => _isFriendsSelected;
        set { SetField(ref _isFriendsSelected, value); Notify(nameof(IsRequestsSelected)); }
    }
    public bool IsRequestsSelected { get => !IsFriendsSelected; set => IsFriendsSelected = !value; }

    private static readonly RelayCommand _unavailable = new RelayCommand(_ => { }, _ => false);
    public RelayCommand ReadyCommand => _unavailable;
    public RelayCommand SendCodeCommand => _unavailable;
    public RelayCommand UpdatePasswordCommand => _unavailable;
    public RelayCommand EditCommand => _unavailable;
    public RelayCommand CancelEditingCommand => _unavailable;
    public RelayCommand SaveCommand => _unavailable;
    public RelayCommand InstagramCommand => _unavailable;
    public RelayCommand FacebookCommand => _unavailable;
    public RelayCommand AddFriendCommand => _unavailable;
    public RelayCommand RemoveFriendCommand => _unavailable;
    public RelayCommand AcceptRequestCommand => _unavailable;
    public RelayCommand RejectRequestCommand => _unavailable;
}
