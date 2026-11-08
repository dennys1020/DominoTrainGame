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
    private readonly IRemoteGameServices _gameServices;
    private string _email;
    private string _username;
    private string _password;

    public SignUpViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
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

    public RelayCommand LogInCommand { get; }

    public AsyncRelayCommand SignUpCommand { get; }

    private async Task SignUpAsync()
    {
        SignUpInputStatus inputStatus = SignUpInputValidator.Validate(Username, Email, Password);

        if (inputStatus == SignUpInputStatus.Valid)
        {
            await SubmitSignUpAsync();
        }
        else
        {
            WindowService.ShowMessage(SignUpMessageKeys.GetInputMessageKey(inputStatus));
        }
    }

    private async Task SubmitSignUpAsync()
    {
        SignUpResult signUpResult = await RunWhileBusyAsync(
            () => _gameServices.SignUpAsync(Username.Trim(), Email.Trim(), Password));
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
            WindowService.ShowMessage(SignUpMessageKeys.GetResultMessageKey(signUpResult));
        }
    }
}
