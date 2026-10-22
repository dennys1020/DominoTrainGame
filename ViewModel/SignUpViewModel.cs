using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;
using DominoTrainGame.Client.ViewModel.Validation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class SignUpViewModel : ScreenViewModel
{
    private const string RequiredFieldsMessageKey = "RequiredFieldsMessage";
    private const string UsernameTooLongMessageKey = "UsernameTooLongMessage";
    private const string EmailTooLongMessageKey = "EmailTooLongMessage";
    private const string EmailInvalidMessageKey = "EmailInvalidMessage";
    private const string PasswordTooShortMessageKey = "PasswordTooShortMessage";
    private const string PasswordTooLongMessageKey = "PasswordTooLongMessage";
    private const string AccountAlreadyExistsMessageKey = "AccountAlreadyExistsMessage";
    private const string SignUpFailedMessageKey = "SignUpFailedMessage";
    private const string ServerUnavailableMessageKey = "ServerUnavailableMessage";
    private const string SignUpSystemErrorMessageKey = "SignUpSystemErrorMessage";
    private const string DatabaseErrorMessageKey = "DatabaseErrorMessage";

    private readonly IRemoteGameServices _gameServices;
    private readonly LocalizationViewModel _localization;
    private string _email;
    private string _username;
    private string _password;
    private bool _isSigningUp;

    public SignUpViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        LocalizationViewModel localization) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _email = string.Empty;
        _username = string.Empty;
        _password = string.Empty;

        LogInCommand = new RelayCommand(() => WindowService.Navigate(Screen.Login));
        SignUpCommand = new AsyncRelayCommand(SignUpAsync);
    }

    public string Email
    {
        get
        {
            return _email;
        }
        set
        {
            SetProperty(ref _email, value ?? string.Empty);
        }
    }

    public string Username
    {
        get
        {
            return _username;
        }
        set
        {
            SetProperty(ref _username, value ?? string.Empty);
        }
    }

    public string Password
    {
        get
        {
            return _password;
        }
        set
        {
            SetProperty(ref _password, value ?? string.Empty);
        }
    }

    public bool IsSigningUp
    {
        get
        {
            return _isSigningUp;
        }
        private set
        {
            SetProperty(ref _isSigningUp, value);
            OnPropertyChanged(nameof(IsContentEnabled));
        }
    }

    public bool IsContentEnabled => !IsSigningUp;

    public RelayCommand LogInCommand { get; }

    public AsyncRelayCommand SignUpCommand { get; }

    private static string GetInputMessageKey(SignUpInputStatus inputStatus)
    {
        string messageKey;

        switch (inputStatus)
        {
            case SignUpInputStatus.UsernameTooLong:
                messageKey = UsernameTooLongMessageKey;
                break;
            case SignUpInputStatus.EmailTooLong:
                messageKey = EmailTooLongMessageKey;
                break;
            case SignUpInputStatus.EmailInvalid:
                messageKey = EmailInvalidMessageKey;
                break;
            case SignUpInputStatus.PasswordTooShort:
                messageKey = PasswordTooShortMessageKey;
                break;
            case SignUpInputStatus.PasswordTooLong:
                messageKey = PasswordTooLongMessageKey;
                break;
            default:
                messageKey = RequiredFieldsMessageKey;
                break;
        }

        return messageKey;
    }

    private static string GetResultMessageKey(SignUpResult signUpResult)
    {
        string messageKey;

        switch (signUpResult)
        {
            case SignUpResult.AlreadyExists:
                messageKey = AccountAlreadyExistsMessageKey;
                break;
            case SignUpResult.InvalidInput:
                messageKey = SignUpFailedMessageKey;
                break;
            case SignUpResult.ServerUnavailable:
                messageKey = ServerUnavailableMessageKey;
                break;
            case SignUpResult.DatabaseUnavailable:
                messageKey = DatabaseErrorMessageKey;
                break;
            default:
                messageKey = SignUpSystemErrorMessageKey;
                break;
        }

        return messageKey;
    }

    private async Task SignUpAsync()
    {
        SignUpInputStatus inputStatus = SignUpInputValidator.Validate(Username, Email, Password);

        if (inputStatus == SignUpInputStatus.Valid)
        {
            await SubmitSignUpAsync();
        }
        else
        {
            ShowLocalizedMessage(GetInputMessageKey(inputStatus));
        }
    }

    private async Task SubmitSignUpAsync()
    {
        SignUpResult signUpResult;
        IsSigningUp = true;

        try
        {
            signUpResult = await _gameServices.SignUpAsync(Username.Trim(), Email.Trim(), Password);
        }
        finally
        {
            IsSigningUp = false;
        }

        HandleSignUpResult(signUpResult);
    }

    private void HandleSignUpResult(SignUpResult signUpResult)
    {
        if (signUpResult == SignUpResult.Success)
        {
            Password = string.Empty;
            WindowService.Navigate(Screen.Verification);
        }
        else
        {
            ShowLocalizedMessage(GetResultMessageKey(signUpResult));
        }
    }

    private void ShowLocalizedMessage(string messageKey)
    {
        string message = _localization[messageKey];
        WindowService.ShowMessage(message);
    }
}
