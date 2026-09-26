using System.Windows;
using DominoTrainGame.Navigation;

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

    private void OpenRoom(object sender, RoutedEventArgs e)
    {
        WindowNavigation.Navigate(this, new RoomWindow(false));
    }
}
