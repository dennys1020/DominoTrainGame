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

public sealed class LoginViewModel : ScreenViewModel
{
    private readonly IRemoteGameServices _gameServices;
    private string _usernameOrEmail;
    private string _password;

    public LoginViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _usernameOrEmail = string.Empty;
        _password = string.Empty;

        LogInCommand = new AsyncRelayCommand(LogInAsync);
        SignUpCommand = new RelayCommand(() => WindowService.Navigate(Screen.SignUp));
        RecoverPasswordCommand = new RelayCommand(() => WindowService.ShowDialog(Screen.RecoverPassword));

        AsyncRelayCommand checkConnectionCommand = new AsyncRelayCommand(CheckConnectionAsync);
        checkConnectionCommand.Execute(null);
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

    public AsyncRelayCommand LogInCommand { get; }

    public RelayCommand SignUpCommand { get; }

    public RelayCommand RecoverPasswordCommand { get; }

    private async Task LogInAsync()
    {
        LoginInputStatus inputStatus = LoginInputValidator.Validate(UsernameOrEmail, Password);

        if (inputStatus == LoginInputStatus.Valid)
        {
            await SubmitLogInAsync();
        }
        else
        {
            WindowService.ShowMessage(LoginMessageKeys.GetInputMessageKey(inputStatus));
        }
    }

    private async Task SubmitLogInAsync()
    {
        LoginResult loginResult = await RunWhileBusyAsync(
            () => _gameServices.LogInAsync(UsernameOrEmail.Trim(), Password));
        HandleLoginResult(loginResult);
    }

    private async Task CheckConnectionAsync()
    {
        ConnectionCheckResult connectionResult = await _gameServices.CheckConnectionAsync();

        if (connectionResult != ConnectionCheckResult.Connected)
        {
            WindowService.ShowMessage(LoginMessageKeys.GetConnectionMessageKey(connectionResult));
        }
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
            WindowService.ShowMessage(LoginMessageKeys.GetResultMessageKey(loginResult));
        }
    }
}
