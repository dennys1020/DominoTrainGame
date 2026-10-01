using DominoTrainGame.Resources.Localization;
using DominoTrainGame.Utils;
using DominoTrainGame.Validator;
using System.Windows;

namespace DominoTrainGame.Views;

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

    //TODO: Implement message box with the result of the password recovery process
    private void NavigateToRecoverPassword(object sender, RoutedEventArgs e)
    {
        RecoverPasswordWindow recoverPasswordWindow = new RecoverPasswordWindow
        {
            Owner = this
        };

        recoverPasswordWindow.ShowDialog();
    }

    private void NavigateToMain_Click(object sender, RoutedEventArgs e)
    {
        string usernameOrEmail = textBoxEmailUserName.Text;
        string password = passwordBoxPassword.Password;

        LoginValidationStatus status = _loginValidator.LogInUser(usernameOrEmail, password);

        if (status == LoginValidationStatus.Success)
        {
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;

            mainWindow.Show();

            this.Close();
        }
        else
        {
            MessageBox.Show(DescribeStatus(status));
        }
    }

    private string DescribeStatus(LoginValidationStatus status)
    {
        string message;
        switch (status)
        {
            case LoginValidationStatus.EmptyFields:
                message = UiStrings.MessageRequiredFields;
                break;
            case LoginValidationStatus.UserNotFound:
                message = UiStrings.MessageUserNotFound;
                break;
            case LoginValidationStatus.IncorrectPassword:
                message = UiStrings.MessageIncorrectPassword;
                break;
            case LoginValidationStatus.DatabaseError:
                message = UiStrings.DatabaseErrorMessage;
                break;
            default:
                message = UiStrings.DatabaseErrorMessage;
                break;
        }
        return message;
    }
}
