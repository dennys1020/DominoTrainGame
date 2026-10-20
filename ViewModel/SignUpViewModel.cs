using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.ViewModels.Commands;
using DominoTrainGame.ViewModels.Common;
using DominoTrainGame.ViewModels.Localization;
using DominoTrainGame.ViewModels.Navigation;
using DominoTrainGame.ViewModels.Validation;

namespace DominoTrainGame.ViewModels;

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
    private string _email;
    private string _username;
    private string _password;
    private bool _isSigningUp;

    public SignUpViewModel(IWindowService windowService, IRemoteGameServices gameServices) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _email = string.Empty;
        _username = string.Empty;
        _password = string.Empty;

        LoginCommand = new RelayCommand(() => WindowService.Navigate(Screen.Login));
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

    public RelayCommand LoginCommand { get; }

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

    private static string GetResultMessageKey(SignupResult signupResult)
    {
        string messageKey;

        switch (signupResult)
        {
            case SignupResult.AlreadyExists:
                messageKey = AccountAlreadyExistsMessageKey;
                break;
            case SignupResult.InvalidInput:
                messageKey = SignUpFailedMessageKey;
                break;
            case SignupResult.ServerUnavailable:
                messageKey = ServerUnavailableMessageKey;
                break;
            case SignupResult.DatabaseUnavailable:
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
        SignupResult signupResult;
        IsSigningUp = true;

        try
        {
            signupResult = await _gameServices.SignUpAsync(Username.Trim(), Email.Trim(), Password);
        }
        finally
        {
            IsSigningUp = false;
        }

        HandleSignUpResult(signupResult);
    }

    private void HandleSignUpResult(SignupResult signupResult)
    {
        if (signupResult == SignupResult.Success)
        {
            Password = string.Empty;
            WindowService.Navigate(Screen.Verification);
        }
        else
        {
            ShowLocalizedMessage(GetResultMessageKey(signupResult));
        }
    }

    private void ShowLocalizedMessage(string messageKey)
    {
        string message = LocalizationViewModel.Instance[messageKey];
        WindowService.ShowMessage(message);
    }
}
