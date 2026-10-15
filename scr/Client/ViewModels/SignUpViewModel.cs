using System.Globalization;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoTrainService;
using DominoTrainGame.Resources.Localization;

namespace DominoTrainGame.ViewModels;

public sealed class SignUpViewModel : ScreenViewModel
{
    private const int MinimumPasswordLength = 12;
    private readonly IGameServices _services;
    private string _email = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _isSigningUp;

    public SignUpViewModel(IGameServices services, IWindowService window) : base(window)
    {
        _services = services;
        SignUpCommand = new AsyncCommand(SignUpAsync);
        LoginCommand = new RelayCommand(() => Window.Navigate(Screen.Login, preservePosition: false));
    }

    public string Email { get => _email; set => SetField(ref _email, value); }
    public string Username { get => _username; set => SetField(ref _username, value); }
    public string Password { get => _password; set => SetField(ref _password, value); }
    public bool IsSigningUp
    {
        get => _isSigningUp;
        private set { SetField(ref _isSigningUp, value); Notify(nameof(IsContentEnabled)); }
    }
    public bool IsContentEnabled => !IsSigningUp;
    public AsyncCommand SignUpCommand { get; }
    public RelayCommand LoginCommand { get; }

    private async Task SignUpAsync()
    {
        User user = new User
        {
            Username = Username,
            Email = Email?.Trim(),
            Password = Password,
            Language = Localization.Culture.TwoLetterISOLanguageName
        };

        IsSigningUp = true;
        try
        {
            RegistrationStatus status = await _services.CheckAvailabilityAsync(user);
            if (Window.IsVisible)
            {
                if (status == RegistrationStatus.Success)
                {
                    await VerifyAndRegisterAsync(user);
                }
                else
                {
                    Window.ShowMessage(DescribeStatus(status));
                }
            }
        }
        finally { IsSigningUp = false; }
    }

    private async Task VerifyAndRegisterAsync(User user)
    {
        VerificationSessionDto session = await _services.BeginVerificationAsync(
            new BeginVerificationRequest { Email = user.Email, Language = user.Language });

        if (Window.IsVisible)
        {
            if (session != null && session.WasSent)
            {
                VerificationRequest request = new VerificationRequest(user.Email, session.SessionId);
                if (Window.ShowDialog(Screen.Verification, request) == true && Window.IsVisible)
                {
                    user.VerificationId = session.SessionId;
                    RegistrationStatus status = await _services.RegisterUserAsync(user);
                    ShowRegistrationResult(status);
                }
            }
            else
            {
                Window.ShowMessage(UiStrings.VerificationCodeSendErrorMessage);
            }
        }
    }

    private void ShowRegistrationResult(RegistrationStatus status)
    {
        if (Window.IsVisible)
        {
            if (status == RegistrationStatus.Success)
            {
                Password = string.Empty;
                Window.ShowMessage(UiStrings.RegistrationSuccessMessage);
                Window.Navigate(Screen.Login, preservePosition: false);
            }
            else
            {
                Window.ShowMessage(DescribeStatus(status));
            }
        }
    }

    private static string DescribeStatus(RegistrationStatus status)
    {
        string message;
        switch (status)
        {
            case RegistrationStatus.EmptyFields:
                message = UiStrings.RequiredFieldsMessage;
                break;
            case RegistrationStatus.InvalidEmail:
                message = UiStrings.InvalidEmailMessage;
                break;
            case RegistrationStatus.PasswordTooShort:
                message = string.Format(CultureInfo.CurrentCulture,
                    UiStrings.InvalidPasswordMessage, MinimumPasswordLength);
                break;
            case RegistrationStatus.UserAlreadyExists:
                message = UiStrings.MessageUserAlreadyExists;
                break;
            case RegistrationStatus.VerificationRequired:
                message = UiStrings.RegistrationVerificationRequiredMessage;
                break;
            default:
                message = UiStrings.RegistrationSystemErrorMessage;
                break;
        }

        return message;
    }
}
