using System.Windows;
using DominoTrainGame.Navigation;

namespace DominoTrainGame.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenNewGame(object sender, RoutedEventArgs e)
    {
        WindowNavigation.Navigate(this, new NewGameWindow());
    }

    private void OpenResults(object sender, RoutedEventArgs e)
    {
        WindowNavigation.Navigate(this, new MatchResultsWindow());
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

    private void OpenChangePassword(object sender, RoutedEventArgs e)
    {
        ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow
        {
            Owner = this
        };

        changePasswordWindow.ShowDialog();
    }

    private void OpenDeleteAccount(object sender, RoutedEventArgs e)
    {
        DeleteAccountWindow deleteAccountWindow = new DeleteAccountWindow
        {
            Owner = this
        };

        deleteAccountWindow.ShowDialog();
    }

    private void OpenProfile(object sender, RoutedEventArgs e)
    {
        ProfileWindow profileWindow = new ProfileWindow
        {
            Owner = this
        };
        profileWindow.ShowDialog();
    }

    private void OpenFriendsList(object sender, RoutedEventArgs e)
    {
        FriendsListWindow friendsWindow = new FriendsListWindow
        {
            Owner = this
        };
        friendsWindow.ShowDialog();
    }
}
