using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class VerificationCodeViewModel : ScreenViewModel
{
    private const int CodeLength = 6;
    private const int ResendCooldownSeconds = 60;
    private const string CountdownTimeFormat = @"m\:ss";
    private const string InstructionFormatKey = "VerificationInstructionFormat";
    private const string IncompleteCodeMessageKey = "VerificationCodeIncompleteMessage";
    private const string AccountVerifiedMessageKey = "AccountVerifiedMessage";
    private const string CodeResentMessageKey = "VerificationCodeResentMessage";
    private const string ResendCodeLabelKey = "ResendCodeButtonLabel";
    private const string ResendCountdownFormatKey = "ResendCodeCountdownFormat";

    private static readonly TimeSpan _countdownStep;

    private readonly IRemoteGameServices _gameServices;
    private readonly LocalizationViewModel _localization;
    private readonly AsyncRelayCommand _countdownCommand;
    private CancellationTokenSource _countdownCancellation;
    private string _message = string.Empty;
    private int _remainingResendSeconds;

    static VerificationCodeViewModel()
    {
        _countdownStep = TimeSpan.FromSeconds(1);
    }

    public VerificationCodeViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        LocalizationViewModel localization) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));

        List<VerificationDigitViewModel> digits = Enumerable
            .Range(0, CodeLength)
            .Select(digitIndex => new VerificationDigitViewModel())
            .ToList();
        Digits = new ReadOnlyCollection<VerificationDigitViewModel>(digits);
        Instruction = string.Format(
            CultureInfo.CurrentCulture,
            _localization[InstructionFormatKey],
            _gameServices.PendingVerificationEmail);

        _countdownCancellation = new CancellationTokenSource();
        _countdownCommand = new AsyncRelayCommand(RunResendCountdownAsync);
        VerifyCommand = new AsyncRelayCommand(VerifyAsync);
        ResendCommand = new AsyncRelayCommand(ResendCodeAsync);
        CancelCommand = new RelayCommand(ReturnToSignUp);
        _countdownCommand.Execute(null);
    }

    public ReadOnlyCollection<VerificationDigitViewModel> Digits { get; }

    public string Instruction { get; }

    public string Message
    {
        get
        {
            return _message;
        }
        private set
        {
            SetProperty(ref _message, value ?? string.Empty);
        }
    }

    public int RemainingResendSeconds
    {
        get
        {
            return _remainingResendSeconds;
        }
        private set
        {
            bool hasChanged = SetProperty(ref _remainingResendSeconds, value);

            if (hasChanged)
            {
                OnPropertyChanged(nameof(CanResend));
                OnPropertyChanged(nameof(ResendLabel));
            }
        }
    }

    public bool CanResend => RemainingResendSeconds == 0;

    public string ResendLabel
    {
        get
        {
            string resendLabel = CanResend
                ? _localization[ResendCodeLabelKey]
                : FormatResendCountdown();
            return resendLabel;
        }
    }

    public AsyncRelayCommand VerifyCommand { get; }

    public AsyncRelayCommand ResendCommand { get; }

    public RelayCommand CancelCommand { get; }

    private async Task VerifyAsync()
    {
        string verificationCode = string.Concat(Digits.Select(digit => digit.Text.Trim()));
        bool isCodeComplete = verificationCode.Length == CodeLength;

        if (isCodeComplete)
        {
            EmailVerificationResult verificationResult = await RunWhileBusyAsync(
                _gameServices.VerifyEmailAsync,
                verificationCode);
            HandleVerificationResult(verificationResult);
        }
        else
        {
            Message = _localization[IncompleteCodeMessageKey];
        }
    }

    private async Task ResendCodeAsync()
    {
        EmailVerificationResult resendResult = await RunWhileBusyAsync(_gameServices.ResendVerificationCodeAsync);
        bool wasResent = resendResult == EmailVerificationResult.Success;
        string messageKey = wasResent
            ? CodeResentMessageKey
            : SignUpMessageKeys.GetVerificationMessageKey(resendResult);

        if (wasResent)
        {
            ClearDigits();
            RestartResendCountdown();
        }

        Message = _localization[messageKey];
    }

    private async Task RunResendCountdownAsync()
    {
        CancellationToken cancellationToken = _countdownCancellation.Token;
        RemainingResendSeconds = ResendCooldownSeconds;

        while (RemainingResendSeconds > 0)
        {
            await Task.Delay(_countdownStep, cancellationToken);
            RemainingResendSeconds--;
        }
    }

    private void RestartResendCountdown()
    {
        _countdownCancellation.Cancel();
        _countdownCancellation.Dispose();
        _countdownCancellation = new CancellationTokenSource();
        _countdownCommand.Execute(null);
    }

    private string FormatResendCountdown()
    {
        TimeSpan remainingTime = TimeSpan.FromSeconds(RemainingResendSeconds);
        string formattedTime = remainingTime.ToString(CountdownTimeFormat, CultureInfo.InvariantCulture);
        string countdownLabel = string.Format(
            CultureInfo.CurrentCulture,
            _localization[ResendCountdownFormatKey],
            formattedTime);
        return countdownLabel;
    }

    private void ClearDigits()
    {
        foreach (VerificationDigitViewModel digit in Digits)
        {
            digit.Text = string.Empty;
        }
    }

    private void ReturnToSignUp()
    {
        _countdownCancellation.Cancel();
        WindowService.Navigate(Screen.SignUp);
    }

    private void HandleVerificationResult(EmailVerificationResult verificationResult)
    {
        bool isVerified = verificationResult == EmailVerificationResult.Success;

        if (isVerified)
        {
            _countdownCancellation.Cancel();
            WindowService.ShowMessage(AccountVerifiedMessageKey);
            WindowService.Navigate(Screen.Login);
        }
        else
        {
            Message = _localization[SignUpMessageKeys.GetVerificationMessageKey(verificationResult)];
        }
    }
}
