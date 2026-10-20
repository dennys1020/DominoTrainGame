using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.ViewModels.Commands;
using DominoTrainGame.ViewModels.Common;
using DominoTrainGame.ViewModels.Localization;
using DominoTrainGame.ViewModels.Navigation;
using DominoTrainGame.ViewModels.Validation;

namespace DominoTrainGame.ViewModels;

public sealed class LoginViewModel : ScreenViewModel
{
    private const string RequiredFieldsMessageKey = "RequiredFieldsMessage";
    private const string IdentifierTooLongMessageKey = "IdentifierTooLongMessage";
    private const string PasswordTooLongMessageKey = "PasswordTooLongMessage";
    private const string InvalidCredentialsMessageKey = "InvalidCredentialsMessage";
    private const string LoginFailedMessageKey = "LoginFailedMessage";
    private const string ServerUnavailableMessageKey = "ServerUnavailableMessage";
    private const string LoginSystemErrorMessageKey = "LoginSystemErrorMessage";
    private const string DatabaseErrorMessageKey = "DatabaseErrorMessage";
    private readonly IRemoteGameServices _gameServices;
    private string _usernameOrEmail;
    private string _password;
    private bool _isLoggingIn;

    public LoginViewModel(IWindowService windowService, IRemoteGameServices gameServices) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _usernameOrEmail = string.Empty;
        _password = string.Empty;

        LoginCommand = new AsyncRelayCommand(LogInAsync);
        SignUpCommand = new RelayCommand(() => WindowService.Navigate(Screen.SignUp));
        RecoverPasswordCommand = new RelayCommand(() => WindowService.ShowDialog(Screen.RecoverPassword));
    }

    public string UsernameOrEmail
    {
        get
        {
            return _usernameOrEmail;
        }
        set
        {
            SetProperty(ref _usernameOrEmail, value ?? string.Empty);
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

    public bool IsLoggingIn
    {
        get
        {
            return _isLoggingIn;
        }
        private set
        {
            SetProperty(ref _isLoggingIn, value);
            OnPropertyChanged(nameof(IsContentEnabled));
        }
    }

    public bool IsContentEnabled => !IsLoggingIn;

    public AsyncRelayCommand LoginCommand { get; }

    public RelayCommand SignUpCommand { get; }

    public RelayCommand RecoverPasswordCommand { get; }

    private static string GetInputMessageKey(LoginInputStatus inputStatus)
    {
        string messageKey;

        switch (inputStatus)
        {
            case LoginInputStatus.IdentifierTooLong:
                messageKey = IdentifierTooLongMessageKey;
                break;
            case LoginInputStatus.PasswordTooLong:
                messageKey = PasswordTooLongMessageKey;
                break;
            default:
                messageKey = RequiredFieldsMessageKey;
                break;
        }

        return messageKey;
    }

    private static string GetResultMessageKey(LoginResult loginResult)
    {
        string messageKey;

        switch (loginResult)
        {
            case LoginResult.InvalidCredentials:
                messageKey = InvalidCredentialsMessageKey;
                break;
            case LoginResult.InvalidInput:
                messageKey = LoginFailedMessageKey;
                break;
            case LoginResult.ServerUnavailable:
                messageKey = ServerUnavailableMessageKey;
                break;
            case LoginResult.DatabaseUnavailable:
                messageKey = DatabaseErrorMessageKey;
                break;
            default:
                messageKey = LoginSystemErrorMessageKey;
                break;
        }

        return messageKey;
    }

    private async Task LogInAsync()
    {
        LoginInputStatus inputStatus = LoginInputValidator.Validate(UsernameOrEmail, Password);

        if (inputStatus == LoginInputStatus.Valid)
        {
            await SubmitLogInAsync();
        }
        else
        {
            ShowLocalizedMessage(GetInputMessageKey(inputStatus));
        }
    }

    private async Task SubmitLogInAsync()
    {
        LoginResult loginResult;
        IsLoggingIn = true;

        try
        {
            loginResult = await _gameServices.LogInAsync(UsernameOrEmail.Trim(), Password);
        }
        finally
        {
            IsLoggingIn = false;
        }

        HandleLoginResult(loginResult);
    }

    private void HandleLoginResult(LoginResult loginResult)
    {
        if (loginResult == LoginResult.Success)
        {
            Password = string.Empty;
            WindowService.Navigate(Screen.Main);
        }
        else
        {
            ShowLocalizedMessage(GetResultMessageKey(loginResult));
        }
    }

    private void ShowLocalizedMessage(string messageKey)
    {
        string message = LocalizationViewModel.Instance[messageKey];
        WindowService.ShowMessage(message);
    }
}
