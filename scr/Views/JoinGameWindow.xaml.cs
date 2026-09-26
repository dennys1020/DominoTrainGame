using System.Windows;
using DominoTrainGame.Navigation;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class JoinGameWindow : Window
{
    public JoinGameWindow()
    {
        InitializeComponent();
    }

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
        GameSetupViewModel room = new GameSetupViewModel();
        room.Players.Add(new RoomPlayerViewModel("local-player", string.Empty)
        {
            IsCurrentPlayer = true
        });
        WindowNavigation.Navigate(this, new WaitingRoomWindow(room, false));
    }
}
