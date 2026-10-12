namespace DominoTrainGame.ViewModels;

public sealed class RoomContext
{
    public RoomContext(bool isHost, RoomViewModel room = null)
    {
        IsHost = isHost;
        Room = room ?? new RoomViewModel();
        if (room == null)
        {
            Room.Players.Add(new RoomPlayerViewModel("local-player", string.Empty)
            {
                IsCurrentPlayer = true,
                IsHost = isHost
            });
        }
    }

    public RoomViewModel Room { get; }
    public bool IsHost { get; }
}

public sealed class RoomScreenViewModel : ScreenViewModel
{
    public RoomScreenViewModel(IWindowService window, RoomContext context) : base(window)
    {
        Room = context.Room;
        IsHost = context.IsHost;
        ReadyCommand = new RelayCommand(MarkReady);
        BackCommand = new RelayCommand(() => Window.Navigate(Screen.NewGame));
        ResultsCommand = new RelayCommand(_ => Window.Navigate(Screen.MatchResults, context), _ => IsHost);
    }

    public RoomViewModel Room { get; }
    public bool IsHost { get; }
    public RelayCommand ReadyCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand ResultsCommand { get; }

    private void MarkReady(object parameter)
    {
        if (parameter is RoomPlayerViewModel player && player.IsCurrentPlayer && Room.Players.Contains(player))
        {
            player.IsReady = true;
        }
    }
}

public sealed class MatchResultsScreenViewModel : ScreenViewModel
{
    public MatchResultsScreenViewModel(IWindowService window, RoomContext context) : base(window)
    {
        Results = context.Room.Results;
        BackCommand = new RelayCommand(() => Window.Navigate(Screen.NewGame));
        MainMenuCommand = new RelayCommand(() => Window.Navigate(Screen.Main));
        RoomCommand = new RelayCommand(() => Window.Navigate(Screen.Room, context));
    }

    public MatchResultsViewModel Results { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand MainMenuCommand { get; }
    public RelayCommand RoomCommand { get; }
}
