using System.Windows;

namespace DominoTrainGame.Views;

public partial class SignUpWindow : Window
{
    public SignUpWindow()
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

    private void SignUpButton_Click(object sender, RoutedEventArgs e)
    {
        string email = textBoxEmail.Text;
        string username = textBoxUserName.Text;
        string password = passwordBoxPassword.Password;

        SignUpValidator validator = new SignUpValidator();
        bool isRegistered = validator.TryRegisterUser(username, email, password, out string resultMessage);

        if (!isRegistered)
        {
            MessageBox.Show(resultMessage);

            return;
        }

        MessageBox.Show(resultMessage);

        LogInWindow loginWindow = new LogInWindow();
        Application.Current.MainWindow = loginWindow;

        loginWindow.Show();

        this.Close();
    }

    private void NavigateToLogin_Click(object sender, RoutedEventArgs e)
    {
        LogInWindow loginWindow = new LogInWindow();
        Application.Current.MainWindow = loginWindow;

        loginWindow.Show();

        this.Close();
    }

    private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }
}