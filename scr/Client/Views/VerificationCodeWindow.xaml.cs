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
    private const int SingleDigitInputLength = 1;
    private const int DigitBoxNavigationStep = 1;
    private const int FirstDigitBoxIndex = 0;
    private const int CooldownFinishedSeconds = 0;
    private const char FirstAsciiDigit = '0';
    private const char LastAsciiDigit = '9';

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
        _resendCooldownTimer.Tick += OnResendCooldownTimerTick;
        Closed += (sender, e) => _resendCooldownTimer.Stop();

        StartResendCooldown();
    }

    private void OnVerifyButtonClicked(object sender, RoutedEventArgs e)
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

    private void OnCancelButtonClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private async void OnResendCodeClicked(object sender, RoutedEventArgs e)
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

    private void OnResendCooldownTimerTick(object sender, EventArgs e)
    {
        UpdateResendCountdown();
    }

    private void UpdateResendCountdown()
    {
        int remainingSeconds = ResendCooldownSeconds - (int)(DateTime.UtcNow - _lastSentAtUtc).TotalSeconds;

        if (remainingSeconds <= CooldownFinishedSeconds)
        {
            _resendCooldownTimer.Stop();
            resendCodeButton.IsEnabled = true;
            resendCodeButton.Content = UiStrings.VerificationCodeResendLabel;
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

    private void OnDigitBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = true;

        int index = GetFocusedIndex();

        bool hasFocusedBox = index >= FirstDigitBoxIndex;
        bool canAcceptInput = hasFocusedBox && e.Text.Length == SingleDigitInputLength;

        if (canAcceptInput && IsAsciiDigit(e.Text[FirstDigitBoxIndex]))
        {
            _digitBoxes[index].Text = e.Text;
            FocusDigitBox(index + DigitBoxNavigationStep);
        }
    }

    private void OnDigitBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        int index = GetFocusedIndex();

        if (index >= FirstDigitBoxIndex)
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
                FocusDigitBox(index - DigitBoxNavigationStep);
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                FocusDigitBox(index + DigitBoxNavigationStep);
            }
        }
    }

    private void OnDigitBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        TextBox focusedBox = e.NewFocus as TextBox;

        if (focusedBox != null)
        {
            focusedBox.SelectAll();
        }
    }

    private void OnDigitBoxPasting(object sender, DataObjectPastingEventArgs e)
    {
        e.CancelCommand();

        int index = GetFocusedIndex();
        string pastedText = e.DataObject.GetData(DataFormats.UnicodeText) as string;

        if (index >= FirstDigitBoxIndex && pastedText != null)
        {
            string digits = new string(pastedText.Where(IsAsciiDigit).ToArray());
            int firstIndex = digits.Length >= CodeLength ? FirstDigitBoxIndex : index;

            for (int offset = 0; offset < digits.Length && firstIndex + offset < CodeLength; offset++)
            {
                _digitBoxes[firstIndex + offset].Text = digits[offset].ToString();
            }

            FocusDigitBox(Math.Min(firstIndex + digits.Length, CodeLength - DigitBoxNavigationStep));
        }
    }

    private int GetFocusedIndex()
    {
        int focusedIndex;
        focusedIndex = Array.IndexOf(_digitBoxes, Keyboard.FocusedElement as TextBox);
        return focusedIndex;
    }

    private void FocusDigitBox(int index)
    {
        if (index >= FirstDigitBoxIndex && index < CodeLength)
        {
            _digitBoxes[index].Focus();
        }
    }

    private void FocusFirstEmptyBox()
    {
        TextBox firstEmptyBox = _digitBoxes.FirstOrDefault(digitBox => string.IsNullOrEmpty(digitBox.Text));

        if (firstEmptyBox != null)
        {
            firstEmptyBox.Focus();
        }
    }

    private void DeleteBackward(int index)
    {
        if (!string.IsNullOrEmpty(_digitBoxes[index].Text))
        {
            _digitBoxes[index].Clear();
        }
        else if (index > FirstDigitBoxIndex)
        {
            _digitBoxes[index - DigitBoxNavigationStep].Clear();
            FocusDigitBox(index - DigitBoxNavigationStep);
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
        bool isAscii;
        isAscii = character >= FirstAsciiDigit && character <= LastAsciiDigit;
        return isAscii;
    }
}
