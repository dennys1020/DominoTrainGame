using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoTrainService;
using DominoTrainGame.Resources.Localization;

namespace DominoTrainGame.ViewModels;

public sealed class LoginViewModel : ScreenViewModel
{
    private readonly IGameServices _services;
    private string _usernameOrEmail = string.Empty;
    private string _password = string.Empty;
    private bool _isLoggingIn;

    public LoginViewModel(IGameServices services, IWindowService window) : base(window)
    {
        _services = services;
        LoginCommand = new AsyncCommand(LogInAsync);
        SignUpCommand = new RelayCommand(() => Window.Navigate(Screen.SignUp, preservePosition: false));
        RecoverPasswordCommand = new RelayCommand(() => Window.ShowDialog(Screen.RecoverPassword));
    }

    public string UsernameOrEmail { get => _usernameOrEmail; set => SetField(ref _usernameOrEmail, value); }
    public string Password { get => _password; set => SetField(ref _password, value); }
    public bool IsLoggingIn
    {
        get => _isLoggingIn;
        private set { SetField(ref _isLoggingIn, value); Notify(nameof(IsContentEnabled)); }
    }
    public bool IsContentEnabled => !IsLoggingIn;
    public AsyncCommand LoginCommand { get; }
    public RelayCommand SignUpCommand { get; }
    public RelayCommand RecoverPasswordCommand { get; }

    private async Task LogInAsync()
    {
        string identifier = UsernameOrEmail;
        string password = Password;
        IsLoggingIn = true;
        try
        {
            LoginStatus status = await _services.LogInAsync(identifier, password);

            if (Window.IsVisible)
            {
                if (status == LoginStatus.Success)
                {
                    Window.Navigate(Screen.Main, preservePosition: false);
                }
                else
                {
                    Window.ShowMessage(DescribeStatus(status));
                }
            }
        }
        finally { IsLoggingIn = false; }
    }

    private static string DescribeStatus(LoginStatus status)
    {
        string message;

        switch (status)
        {
            case LoginStatus.EmptyFields:
                message = UiStrings.RequiredFieldsMessage;
                break;
            case LoginStatus.UserNotFound:
                message = UiStrings.MessageUserNotFound;
                break;
            case LoginStatus.IncorrectPassword:
                message = UiStrings.MessageIncorrectPassword;
                break;
            default:
                message = UiStrings.LoginSystemErrorMessage;
                break;
        }

        return message;
    }
}
