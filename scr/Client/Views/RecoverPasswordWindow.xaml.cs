using System;
using System.Windows;
using DominoTrainGame.Resources.Localization;
using DominoTrainGame.Utils;

namespace DominoTrainGame.Views;

public partial class RecoverPasswordWindow : Window
{
    private readonly RecoverPasswordValidator _validator;
    private int _playerId;

    public RecoverPasswordWindow()
    {
        InitializeComponent();
        _validator = new RecoverPasswordValidator();
    }

    private async void SendCodeButton_Click(object sender, RoutedEventArgs e)
    {
        RecoverPasswordValidationStatus status = _validator.FindAccount(
            EmailOrUsernameTextBox.Text, out int playerId, out string email);

        TextBlockMessage.Text = DescribeStatus(status);

        if (status == RecoverPasswordValidationStatus.Success)
        {
            _playerId = playerId;

            VerificationCodeService codeService = new VerificationCodeService();
            string code = codeService.GenerateCode();

            SendCodeButton.IsEnabled = false;
            bool wasSent = await new EmailSender().SendVerificationCodeAsync(email, code);
            SendCodeButton.IsEnabled = true;

            if (wasSent)
            {
                VerificationCodeWindow verificationWindow = new VerificationCodeWindow(email, codeService)
                {
                    Owner = this
                };

                bool? wasVerified = verificationWindow.ShowDialog();

                if (wasVerified is true)
                {
                    IdentifyStep.Visibility = Visibility.Collapsed;
                    NewPasswordStep.Visibility = Visibility.Visible;
                    TextBlockMessage.Text = string.Empty;
                }
            }
            else
            {
                TextBlockMessage.Text = UiStrings.VerificationCodeSendErrorMessage;
            }
        }
    }

    private void UpdatePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        RecoverPasswordValidationStatus status = _validator.UpdatePassword(
            _playerId, NewPasswordBox.Password, ConfirmPasswordBox.Password);

        TextBlockMessage.Text = DescribeStatus(status);

        if (status == RecoverPasswordValidationStatus.Success)
        {
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private static string DescribeStatus(RecoverPasswordValidationStatus status)
    {
        string message;

        switch (status)
        {
            case RecoverPasswordValidationStatus.Success:
                message = UiStrings.RecoverPasswordSuccessMessage;
                break;
            case RecoverPasswordValidationStatus.UserNotFound:
                message = UiStrings.MessageUserNotFound;
                break;
            case RecoverPasswordValidationStatus.PasswordTooShort:
                message = string.Format(UiStrings.InvalidPasswordMessage, RecoverPasswordValidator.MinimumPasswordLength);
                break;
            case RecoverPasswordValidationStatus.PasswordsDoNotMatch:
                message = UiStrings.PasswordsDoNotMatchMessage;
                break;
            case RecoverPasswordValidationStatus.DatabaseError:
                message = UiStrings.DatabaseErrorMessage;
                break;
            default:
                message = UiStrings.MessageRequiredFields;
                break;
        }

        return message;
    }
}