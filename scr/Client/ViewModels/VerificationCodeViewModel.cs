using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Resources.Localization;
using DominoTrainGame.Client.Models.DominoTrainService;

namespace DominoTrainGame.ViewModels;

public sealed class VerificationRequest
{
    public VerificationRequest(string email, Guid sessionId) { Email = email; SessionId = sessionId; }
    public string Email { get; }
    public Guid SessionId { get; }
}

public sealed class VerificationDigit : ObservableViewModel
{
    private string _text = string.Empty;
    public string Text { get => _text; set => SetField(ref _text, value); }
}

public sealed class VerificationCodeViewModel : ScreenViewModel
{
    private const int CodeLength = 6;
    private const int ResendCooldownSeconds = 30;
    private readonly VerificationRequest _request;
    private readonly IGameServices _services;
    private DateTime _lastSentAtUtc = DateTime.UtcNow;
    private string _message = string.Empty;
    private string _resendLabel;
    private bool _canResend;

    public VerificationCodeViewModel(IGameServices services, IWindowService window, VerificationRequest request) : base(window)
    {
        _services = services;
        _request = request;
        Instruction = string.Format(CultureInfo.CurrentCulture, UiStrings.VerificationCodeSentMessage, request.Email);
        Digits = new ObservableCollection<VerificationDigit>(Enumerable.Range(0, CodeLength).Select(_ => new VerificationDigit()));
        VerifyCommand = new AsyncCommand(VerifyAsync);
        ResendCommand = new AsyncCommand(ResendAsync);
        UpdateCountdown();
    }

    public event EventHandler FocusFirstEmptyRequested;
    public event EventHandler FocusFirstDigitRequested;
    public string Instruction { get; }
    public ObservableCollection<VerificationDigit> Digits { get; }
    public string Message { get => _message; private set => SetField(ref _message, value); }
    public string ResendLabel { get => _resendLabel; private set => SetField(ref _resendLabel, value); }
    public bool CanResend { get => _canResend; private set => SetField(ref _canResend, value); }
    public AsyncCommand VerifyCommand { get; }
    public AsyncCommand ResendCommand { get; }

    private async Task VerifyAsync()
    {
        string enteredCode = string.Concat(Digits.Select(digit => digit.Text));
        if (enteredCode.Length < CodeLength)
        {
            Message = UiStrings.VerificationCodeInvalidMessage;
            FocusFirstEmptyRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        VerificationCodeStatus status = await _services.VerifyCodeAsync(_request.SessionId, enteredCode);
        if (status == VerificationCodeStatus.Valid) { Window.Close(true); }
        else if (status == VerificationCodeStatus.Invalid)
        {
            Message = UiStrings.VerificationCodeInvalidMessage;
            ClearDigits();
        }
        else if (status == VerificationCodeStatus.ServiceUnavailable)
        {
            Message = UiStrings.VerificationCodeSendErrorMessage;
        }
        else
        {
            Window.ShowMessage(status == VerificationCodeStatus.Expired
                ? UiStrings.VerificationCodeExpiredMessage : UiStrings.VerificationCodeTooManyAttemptsMessage);
            Window.Close(false);
        }
    }

    private async Task ResendAsync()
    {
        bool wasSent = await _services.ResendVerificationAsync(_request.SessionId);
        if (wasSent)
        {
            Message = UiStrings.VerificationCodeResentMessage;
            ClearDigits();
            _lastSentAtUtc = DateTime.UtcNow;
            UpdateCountdown();
        }
        else { Message = UiStrings.VerificationCodeSendErrorMessage; }
    }

    public void UpdateCountdown()
    {
        int remainingSeconds = ResendCooldownSeconds - (int)(DateTime.UtcNow - _lastSentAtUtc).TotalSeconds;
        CanResend = remainingSeconds <= 0;
        ResendLabel = CanResend ? UiStrings.VerificationCodeResendLabel : string.Format(
            CultureInfo.CurrentCulture, UiStrings.VerificationCodeResendWaitMessage, remainingSeconds);
    }

    private void ClearDigits()
    {
        foreach (VerificationDigit digit in Digits) { digit.Text = string.Empty; }
        FocusFirstDigitRequested?.Invoke(this, EventArgs.Empty);
    }
}
