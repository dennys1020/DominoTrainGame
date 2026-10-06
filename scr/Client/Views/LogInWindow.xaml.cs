using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DominoTrainGame.Resources.Localization;
using DominoTrainGame.Validator;

namespace DominoTrainGame.Views;

public partial class LogInWindow : Window
{
    private readonly LoginValidator _loginValidator;
    private bool _isLoggingIn;

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

    private void OnNavigateToSignUpClicked(object sender, RoutedEventArgs e)
    {
        SignUpWindow signUpWindow = new SignUpWindow();
        Application.Current.MainWindow = signUpWindow;

        signUpWindow.Show();

        this.Close();
    }

    // TODO: Display the password recovery result when the dialog closes.
    private void NavigateToRecoverPassword(object sender, RoutedEventArgs e)
    {
        RecoverPasswordWindow recoverPasswordWindow = new RecoverPasswordWindow
        {
            Owner = this
        };

        recoverPasswordWindow.ShowDialog();
    }

    private async void OnNavigateToMainClicked(object sender, RoutedEventArgs e)
    {
        if (_isLoggingIn)
        {
            return;
        }

        string usernameOrEmail = textBoxEmailUserName.Text;
        string password = passwordBoxPassword.Password;
        Cursor previousCursor = Cursor;
        _isLoggingIn = true;
        loginContent.IsEnabled = false;
        Cursor = Cursors.Wait;

        try
        {
            LoginValidationStatus status = await Task.Run(
                () => _loginValidator.LogInUser(usernameOrEmail, password));

            if (!IsVisible)
            {
                return;
            }

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
        finally
        {
            _isLoggingIn = false;
            loginContent.IsEnabled = true;
            Cursor = previousCursor;
        }
    }

    private string DescribeStatus(LoginValidationStatus status)
    {
        string message;
        switch (status)
        {
            case LoginValidationStatus.EmptyFields:
                message = UiStrings.RequiredFieldsMessage;
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
