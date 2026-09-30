using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading; 
using DominoTrainGame.Resources.Localization;
using DominoTrainGame.Utils;

namespace DominoTrainGame.Views;

public partial class VerificationCodeWindow : Window
{
    private const int CodeLength = 6;
    private const int ResendCooldownSeconds = 30;
    private const int CountdownTickSeconds = 1;  
    private readonly string _email;
    private readonly VerificationCodeService _codeService;
    private readonly TextBox[] _digitBoxes;
    private readonly DispatcherTimer _resendCooldownTimer;
    private DateTime _lastSentAtUtc = DateTime.UtcNow;
    private bool _isResending;

    public VerificationCodeWindow(string email, VerificationCodeService codeService)
    {
        InitializeComponent();

        _email = email;
        _codeService = codeService;
        _digitBoxes = new[] { digitBox1, digitBox2, digitBox3, digitBox4, digitBox5, digitBox6 };

        textBlockInstruction.Text = string.Format(
            CultureInfo.CurrentCulture,
            UiStrings.VerificationCodeSentMessage,
            _email);

        _resendCooldownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(CountdownTickSeconds)
        };
        _resendCooldownTimer.Tick += ResendCooldownTimer_Tick;
        Closed += (sender, e) => _resendCooldownTimer.Stop();

        StartResendCooldown();
    }

    private void VerifyButton_Click(object sender, RoutedEventArgs e)
    {
        string enteredCode = string.Concat(_digitBoxes.Select(digitBox => digitBox.Text));

        if (enteredCode.Length < CodeLength)
        {
            textBlockMessage.Text = UiStrings.VerificationCodeInvalidMessage;
            FocusFirstEmptyBox();
        }
        else
        {
            ShowVerificationResult(_codeService.Verify(enteredCode));
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private async void ResendCode_Click(object sender, RoutedEventArgs e)
    {
        if (!_isResending)
        {
            await ResendCodeAsync();
        }
    }

    private async Task ResendCodeAsync()
    {
        _isResending = true;

        string code = _codeService.GenerateCode();
        bool wasSent = await new EmailSender().SendVerificationCodeAsync(_email, code);

        _isResending = false;

        if (wasSent)
        {
            textBlockMessage.Text = UiStrings.VerificationCodeResentMessage;
            ClearDigits();
            StartResendCooldown();
        }
        else
        {
            textBlockMessage.Text = UiStrings.VerificationCodeSendErrorMessage;
        }
    }

    private void StartResendCooldown()
    {
        _lastSentAtUtc = DateTime.UtcNow;
        resendCodeButton.IsEnabled = false;
        _resendCooldownTimer.Start();
        UpdateResendCountdown();
    }

    private void ResendCooldownTimer_Tick(object sender, EventArgs e)
    {
        UpdateResendCountdown();
    }

    private void UpdateResendCountdown()
    {
        int remainingSeconds = ResendCooldownSeconds - (int)(DateTime.UtcNow - _lastSentAtUtc).TotalSeconds;

        if (remainingSeconds <= 0)
        {
            _resendCooldownTimer.Stop();
            resendCodeButton.IsEnabled = true;
            resendCodeButton.Content = UiStrings.VerificationCodeResendLabel;   // en vez de ResendCodeButtonLabel
        }
        else
        {
            resendCodeButton.Content = string.Format(
                CultureInfo.CurrentCulture,
                UiStrings.VerificationCodeResendWaitMessage,
                remainingSeconds);
        }
    }

    private void ShowVerificationResult(VerificationCodeStatus status)
    {
        if (status == VerificationCodeStatus.Valid)
        {
            DialogResult = true;
        }
        else if (status == VerificationCodeStatus.Invalid)
        {
            textBlockMessage.Text = UiStrings.VerificationCodeInvalidMessage;
            ClearDigits();
        }
        else
        {
            string message = status == VerificationCodeStatus.Expired
                ? UiStrings.VerificationCodeExpiredMessage
                : UiStrings.VerificationCodeTooManyAttemptsMessage;

            MessageBox.Show(message);
            DialogResult = false;
        }
    }

    private void DigitBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = true;

        int index = GetFocusedIndex();

        if (index >= 0 && e.Text.Length == 1 && IsAsciiDigit(e.Text[0]))
        {
            _digitBoxes[index].Text = e.Text;
            FocusDigitBox(index + 1);
        }
    }

    private void DigitBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        int index = GetFocusedIndex();

        if (index >= 0)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                e.Handled = true;
                DeleteBackward(index);
            }
            else if (e.Key == Key.Delete)
            {
                e.Handled = true;
                _digitBoxes[index].Clear();
            }
            else if (e.Key == Key.Left)
            {
                e.Handled = true;
                FocusDigitBox(index - 1);
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                FocusDigitBox(index + 1);
            }
        }
    }

    private void DigitBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        TextBox focusedBox = e.NewFocus as TextBox;

        if (focusedBox != null)
        {
            focusedBox.SelectAll();
        }
    }

    private void DigitBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        e.CancelCommand();

        int index = GetFocusedIndex();
        string pastedText = e.DataObject.GetData(DataFormats.UnicodeText) as string;

        if (index >= 0 && pastedText != null)
        {
            string digits = new string(pastedText.Where(IsAsciiDigit).ToArray());
            int firstIndex = digits.Length >= CodeLength ? 0 : index;

            for (int offset = 0; offset < digits.Length && firstIndex + offset < CodeLength; offset++)
            {
                _digitBoxes[firstIndex + offset].Text = digits[offset].ToString();
            }

            FocusDigitBox(Math.Min(firstIndex + digits.Length, CodeLength - 1));
        }
    }

    private int GetFocusedIndex()
    {
        return Array.IndexOf(_digitBoxes, Keyboard.FocusedElement as TextBox);
    }

    private void FocusDigitBox(int index)
    {
        if (index >= 0 && index < CodeLength)
        {
            _digitBoxes[index].Focus();
        }
    }

    private void FocusFirstEmptyBox()
    {
        TextBox firstEmptyBox = _digitBoxes.FirstOrDefault(digitBox => digitBox.Text.Length == 0);

        if (firstEmptyBox != null)
        {
            firstEmptyBox.Focus();
        }
    }

    private void DeleteBackward(int index)
    {
        if (_digitBoxes[index].Text.Length > 0)
        {
            _digitBoxes[index].Clear();
        }
        else if (index > 0)
        {
            _digitBoxes[index - 1].Clear();
            FocusDigitBox(index - 1);
        }
    }

    private void ClearDigits()
    {
        foreach (TextBox digitBox in _digitBoxes)
        {
            digitBox.Clear();
        }

        digitBox1.Focus();
    }

    private static bool IsAsciiDigit(char character)
    {
        return character >= '0' && character <= '9';
    }
}