using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoTrainService;
using DominoTrainGame.Resources.Localization;

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
    private string _messageKey = string.Empty;
    private int _remainingSeconds;
    private bool _isBusy;

    public VerificationCodeViewModel(IGameServices services, IWindowService window, VerificationRequest request) : base(window)
    {
        _services = services;
        _request = request;
        Digits = new ObservableCollection<VerificationDigit>();
        for (int index = 0; index < CodeLength; index++) { Digits.Add(new VerificationDigit()); }
        VerifyCommand = new AsyncCommand(VerifyAsync);
        ResendCommand = new AsyncCommand(ResendAsync);
        UpdateCountdown();
    }

    public event EventHandler FocusFirstEmptyRequested;
    public event EventHandler FocusFirstDigitRequested;
    public string Instruction => string.Format(Localization.Culture, UiStrings.VerificationCodeSentMessage, _request.Email);
    public ObservableCollection<VerificationDigit> Digits { get; }
    public string Message => Localization[_messageKey];
    public string ResendLabel => _remainingSeconds <= 0 ? UiStrings.VerificationCodeResendLabel
        : string.Format(Localization.Culture, UiStrings.VerificationCodeResendWaitMessage, _remainingSeconds);
    public bool CanResend => !_isBusy && _remainingSeconds <= 0;
    public bool IsContentEnabled => !_isBusy;
    public AsyncCommand VerifyCommand { get; }
    public AsyncCommand ResendCommand { get; }

    private async Task VerifyAsync()
    {
        if (!_isBusy)
        {
            string enteredCode = string.Concat(Digits.Select(GetDigitText));
            if (enteredCode.Length != CodeLength)
            {
                SetMessage(nameof(UiStrings.VerificationCodeInvalidMessage));
                FocusFirstEmptyRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                SetBusy(true);
                try
                {
                    VerificationStatus status = await _services.VerifyCodeAsync(_request.SessionId, enteredCode);
                    if (Window.IsVisible) { ShowVerificationResult(status); }
                }
                finally { SetBusy(false); }
            }
        }
    }

    private void ShowVerificationResult(VerificationStatus status)
    {
        if (status == VerificationStatus.Valid) { Window.Close(true); }
        else if (status == VerificationStatus.Invalid)
        {
            SetMessage(nameof(UiStrings.VerificationCodeInvalidMessage));
            ClearDigits();
        }
        else if (status == VerificationStatus.Expired || status == VerificationStatus.TooManyAttempts)
        {
            Window.ShowMessage(status == VerificationStatus.Expired
                ? UiStrings.VerificationCodeExpiredMessage : UiStrings.VerificationCodeTooManyAttemptsMessage);
            Window.Close(false);
        }
        else
        {
            SetMessage(nameof(UiStrings.VerificationSystemErrorMessage));
        }
    }

    private async Task ResendAsync()
    {
        UpdateCountdown();
        if (CanResend)
        {
            SetBusy(true);
            _lastSentAtUtc = DateTime.UtcNow;
            try
            {
                bool wasSent = await _services.ResendVerificationAsync(_request.SessionId);
                if (Window.IsVisible)
                {
                    SetMessage(wasSent ? nameof(UiStrings.VerificationCodeResentMessage)
                        : nameof(UiStrings.VerificationCodeSendErrorMessage));
                    if (wasSent) { ClearDigits(); }
                }
            }
            finally { SetBusy(false); }
        }
    }

    public void UpdateCountdown()
    {
        _remainingSeconds = Math.Max(0,
            ResendCooldownSeconds - (int)(DateTime.UtcNow - _lastSentAtUtc).TotalSeconds);
        Notify(nameof(CanResend));
        Notify(nameof(ResendLabel));
    }

    private void SetBusy(bool isBusy)
    {
        _isBusy = isBusy;
        Notify(nameof(IsContentEnabled));
        UpdateCountdown();
    }

    private void SetMessage(string resourceKey)
    {
        _messageKey = resourceKey;
        Notify(nameof(Message));
    }

    private void ClearDigits()
    {
        foreach (VerificationDigit digit in Digits) { digit.Text = string.Empty; }
        FocusFirstDigitRequested?.Invoke(this, EventArgs.Empty);
    }

    private static string GetDigitText(VerificationDigit digit)
    {
        return digit.Text;
    }
}
