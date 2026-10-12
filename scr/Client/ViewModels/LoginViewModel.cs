using System.Threading.Tasks;
using DominoTrainGame.Contracts;
using DominoTrainGame.Resources.Localization;
using DominoTrainGame.Validator;

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
            LoginValidationStatus status = await Task.Run(() => _services.LogInUser(identifier, password));
            if (!Window.IsVisible) { return; }
            if (status == LoginValidationStatus.Success)
            {
                Window.Navigate(Screen.Main, preservePosition: false);
            }
            else { Window.ShowMessage(DescribeStatus(status)); }
        }
        finally { IsLoggingIn = false; }
    }

    private string DescribeStatus(LoginValidationStatus status)
    {
        string message;
        switch (status)
        {
            case LoginValidationStatus.EmptyFields:
                message = UiStrings.RequiredFieldsMessage;
                break;
            case LoginValidationStatus.UserNotFound:
                message = UiStrings.MessageUserNotFound;
                break;
            case LoginValidationStatus.IncorrectPassword:
                message = UiStrings.MessageIncorrectPassword;
                break;
            case LoginValidationStatus.DatabaseError:
                message = UiStrings.DatabaseErrorMessage;
                break;
            default:
                message = UiStrings.DatabaseErrorMessage;
                break;
        }
        return message;
    }
}
