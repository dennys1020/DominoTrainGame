using DominoTrainGame.Utils;
using DominoTrainGame.Resources.Localization;
using System.Windows;

namespace DominoTrainGame.Views;

public partial class SignUpWindow : Window
{
    private const int MinimumPasswordLength = 12;
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
        SignUpValidationStatus status = validator.TryRegisterUser(username, email, password);

        if (status != SignUpValidationStatus.Success)
        {
            MessageBox.Show(DescribeStatus(status));

            return;
        }

        MessageBox.Show(UiStrings.RegisterSucessMessage);

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

    private string DescribeStatus(SignUpValidationStatus status)
    {
        switch (status)
        {
            case SignUpValidationStatus.EmptyFields:
                return UiStrings.EmptyFieldsMessage;
            case SignUpValidationStatus.InvalidEmail:
                return UiStrings.MesssageInvalidEmail;
            case SignUpValidationStatus.PasswordTooShort:
                return string.Format(UiStrings.InvalidPasswordMessage, MinimumPasswordLength);
            case SignUpValidationStatus.UserAlreadyExists:
                return UiStrings.MessageUserAlreadyExists;
            case SignUpValidationStatus.DatabaseError:
                return UiStrings.DatabaseErrorMessage;
            default:
                return UiStrings.DatabaseErrorMessage;
        }
    }
}