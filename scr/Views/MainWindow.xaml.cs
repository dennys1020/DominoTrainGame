using DominoTrainGame.Views;
using System.Windows;

namespace DominoTrainGame.Views
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow
            {
                Owner = this
            };

            settingsWindow.ShowDialog();
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
            FriendsListWindow friendsListWindow = new FriendsListWindow
            {
                Owner = this
            };
            friendsListWindow.ShowDialog();
        }
    }
}
