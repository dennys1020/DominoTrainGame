using DominoTrainGame.Views;
using System.Windows;

namespace DominoTrainGame.Views
{

    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow
            {
                Owner = this
            };

            settingsWindow.ShowDialog();
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
