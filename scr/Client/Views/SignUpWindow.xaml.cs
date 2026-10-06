using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DominoTrainGame.Resources.Localization;
using DominoTrainGame.Utils;

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

    private async void OnSignUpButtonClicked(object sender, RoutedEventArgs e)
    {
        Button signUpButton = (Button)sender;
        string email = textBoxEmail.Text.Trim();
        string username = textBoxUserName.Text;
        string password = passwordBoxPassword.Password;

        SignUpValidator validator = new SignUpValidator();
        SignUpValidationStatus status = validator.CheckAvailability(username, email, password);

        if (status != SignUpValidationStatus.Success)
        {
            MessageBox.Show(DescribeStatus(status));
        }
        else
        {
            signUpButton.IsEnabled = false;
            bool isEmailVerified = await VerifyEmailAsync(email);
            signUpButton.IsEnabled = true;

            if (isEmailVerified)
            {
                CompleteRegistration(username, email, password);
            }
        }
    }

    private async Task<bool> VerifyEmailAsync(string email)
    {
        VerificationCodeService codeService = new VerificationCodeService();
        string code = codeService.GenerateCode();
        bool isVerified = false;

        bool wasSent = await new EmailSender().SendVerificationCodeAsync(email, code);

        if (!wasSent)
        {
            MessageBox.Show(UiStrings.VerificationCodeSendErrorMessage);
        }
        else
        {
            VerificationCodeWindow verificationWindow = new VerificationCodeWindow(email, codeService)
            {
                Owner = this
            };

            isVerified = verificationWindow.ShowDialog() == true;
        }

        return isVerified;
    }

    private void CompleteRegistration(string username, string email, string password)
    {
        SignUpValidator validator = new SignUpValidator();
        SignUpValidationStatus status = validator.RegisterUser(username, email, password);

        if (status != SignUpValidationStatus.Success)
        {
            MessageBox.Show(DescribeStatus(status));
        }
        else
        {
            MessageBox.Show(UiStrings.RegistrationSuccessMessage);

            LogInWindow loginWindow = new LogInWindow();
            Application.Current.MainWindow = loginWindow;

            loginWindow.Show();

            this.Close();
        }
    }

    private void OnNavigateToLoginClicked(object sender, RoutedEventArgs e)
    {
        LogInWindow loginWindow = new LogInWindow();
        Application.Current.MainWindow = loginWindow;

        loginWindow.Show();

        this.Close();
    }

    private void OnTextBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
    }

    private string DescribeStatus(SignUpValidationStatus status)
    {
        string statusMessage;
        switch (status)
        {
            case SignUpValidationStatus.EmptyFields:
                statusMessage = UiStrings.RequiredFieldsMessage;
                break;
            case SignUpValidationStatus.InvalidEmail:
                statusMessage = UiStrings.InvalidEmailMessage;
                break;
            case SignUpValidationStatus.PasswordTooShort:
                statusMessage = string.Format(UiStrings.InvalidPasswordMessage, MinimumPasswordLength);
                break;
            case SignUpValidationStatus.UserAlreadyExists:
                statusMessage = UiStrings.MessageUserAlreadyExists;
                break;
            case SignUpValidationStatus.DatabaseError:
                statusMessage = UiStrings.DatabaseErrorMessage;
                break;
            default:
                statusMessage = UiStrings.DatabaseErrorMessage;
                break;
        }
        return statusMessage;
    }
}
