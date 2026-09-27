using System.Windows;
using DominoTrainGame.Navigation;

namespace DominoTrainGame.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenNewGame(object sender, RoutedEventArgs eventArgs)
    {
        WindowNavigation.Navigate(this, new NewGameWindow());
    }

    private void OpenResults(object sender, RoutedEventArgs eventArgs)
    {
        WindowNavigation.Navigate(this, new MatchResultsWindow());
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
    }*/

    private void OpenChangePassword(object sender, RoutedEventArgs eventArgs)
    {
        ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow
        {
            Owner = this
        };

        changePasswordWindow.ShowDialog();
    }

    private void OpenDeleteAccount(object sender, RoutedEventArgs eventArgs)
    {
        DeleteAccountWindow deleteAccountWindow = new DeleteAccountWindow
        {
            Owner = this
        };

        deleteAccountWindow.ShowDialog();
    }

    private void OpenProfile(object sender, RoutedEventArgs eventArgs)
    {
        ProfileWindow profileWindow = new ProfileWindow
        {
            Owner = this
        };
        profileWindow.ShowDialog();
    }

    private void OpenFriendsList(object sender, RoutedEventArgs eventArgs)
    {
        FriendsListWindow friendsWindow = new FriendsListWindow
        {
            Owner = this
        };
        friendsWindow.ShowDialog();
    }
}
