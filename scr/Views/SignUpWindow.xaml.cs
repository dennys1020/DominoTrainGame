using DominoTrainGame.Utils;
using DominoTrainGame.Resources.Localization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

    private async void SignUpButton_Click(object sender, RoutedEventArgs e)
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
            MessageBox.Show(UiStrings.RegisterSucessMessage);

            LogInWindow loginWindow = new LogInWindow();
            Application.Current.MainWindow = loginWindow;

            loginWindow.Show();

            this.Close();
        }
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

    private void SettingsButton_Loaded(object sender, RoutedEventArgs e)
    {

    }
}