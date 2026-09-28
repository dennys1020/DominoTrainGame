using System.Windows;

namespace DominoTrainGame.Views
{

    public partial class LogInWindow : Window
    {
        private readonly LoginValidator _loginValidator;

        public LogInWindow()
        {
            InitializeComponent();
            _loginValidator = new LoginValidator();
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

        private void NavigateToRecoverPassword(object sender, RoutedEventArgs e)
        {
            RecoverPasswordWindow recoverPassword = new RecoverPasswordWindow();
            Application.Current.MainWindow = recoverPassword;

            recoverPassword.Show();

            this.Close();
        }

        private void NavigateToMain_Click(object sender, RoutedEventArgs e)
        {
            string usernameOrEmail = textBoxEmailUserName.Text;
            string password = passwordBoxPassword.Password;

            bool isLoginSuccessful = _loginValidator.TryLogin(usernameOrEmail, password, out string resultMessage);


            if (!isLoginSuccessful)
            {
                MessageBox.Show(resultMessage);
                return;
            }

            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;

            mainWindow.Show();

            this.Close();
        }
    }

}
