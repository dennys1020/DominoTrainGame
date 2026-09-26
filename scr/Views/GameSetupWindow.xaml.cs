using System.Windows;
using DominoTrainGame.Navigation;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class GameSetupWindow : Window
{
    public GameSetupWindow() : this(new GameSetupViewModel())
    {
    }

    public GameSetupWindow(GameSetupViewModel room)
    {
        Room = room;
        InitializeComponent();
    }

    public GameSetupViewModel Room { get; }

    private void NavigateBack(object sender, RoutedEventArgs e)
    {
        WindowNavigation.Navigate(this, new NewGameWindow());
    }

    private void OpenRules(object sender, RoutedEventArgs e)
    {
        GameRulesWindow rulesWindow = new GameRulesWindow
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false
        };
        rulesWindow.ShowDialog();
    }

    private void OpenWaitingRoom(object sender, RoutedEventArgs e)
    {
        WindowNavigation.Navigate(this, new WaitingRoomWindow(Room, true));
    }

    private void OpenResults(object sender, RoutedEventArgs e)
    {
        WindowNavigation.Navigate(this, new MatchResultsWindow(Room, true));
    }
}
