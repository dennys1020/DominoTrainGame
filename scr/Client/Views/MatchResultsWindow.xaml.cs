using System.Windows;
using DominoTrainGame.Navigation;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class MatchResultsWindow : Window
{
    private readonly RoomViewModel _room;
    private readonly bool _isHost;

    public MatchResultsWindow() : this(new RoomViewModel(), false)
    {
    }

    public MatchResultsWindow(RoomViewModel room, bool isHost)
    {
        _room = room;
        _isHost = isHost;
        Results = room.Results;
        InitializeComponent();
    }

    public MatchResultsViewModel Results
    {
        get;
    }

    private void NavigateBack(object sender, RoutedEventArgs eventArgs)
    {
        WindowNavigation.Navigate(this, new NewGameWindow());
    }

    private void OpenRules(object sender, RoutedEventArgs eventArgs)
    {
        GameRulesWindow rulesWindow = new GameRulesWindow
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false
        };
        rulesWindow.ShowDialog();
    }

    private void OpenMainMenu(object sender, RoutedEventArgs eventArgs)
    {
        WindowNavigation.Navigate(this, new MainWindow());
    }

    private void OpenRoom(object sender, RoutedEventArgs eventArgs)
    {
        WindowNavigation.Navigate(this, new RoomWindow(_room, _isHost));
    }
}
