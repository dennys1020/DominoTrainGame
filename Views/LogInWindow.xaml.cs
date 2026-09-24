using DominoTrainGame.Views;
using System.Windows;

namespace DominoTrainGame.Views
{

    public partial class LogInWindow : Window
    {
        public LogInWindow()
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

        private void NavigateToSignUp_Click(object sender, RoutedEventArgs e)
        {
            SignUpWindow signUpWindow = new SignUpWindow();
            Application.Current.MainWindow = signUpWindow;

            signUpWindow.Show();

            this.Close();
        }

        private void NavigateToMain_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;

            mainWindow.Show();

            this.Close();
        }
    }

}
