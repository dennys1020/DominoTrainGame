using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Resources.Localization;
using DominoTrainGame.Client.Models.DominoTrainService;

namespace DominoTrainGame.ViewModels;

public sealed class SignUpViewModel : ScreenViewModel
{
    private const int MinimumPasswordLength = 12;
    private readonly IGameServices _services;
    private string _email = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;

    public SignUpViewModel(IGameServices services, IWindowService window) : base(window)
    {
        _services = services;
        SignUpCommand = new AsyncCommand(SignUpAsync);
        LoginCommand = new RelayCommand(() => Window.Navigate(Screen.Login, preservePosition: false));
    }

    public string Email { get => _email; set => SetField(ref _email, value); }
    public string Username { get => _username; set => SetField(ref _username, value); }
    public string Password { get => _password; set => SetField(ref _password, value); }
    public AsyncCommand SignUpCommand { get; }
    public RelayCommand LoginCommand { get; }

    private async Task SignUpAsync()
    {
        string email = Email.Trim();
        string username = Username;
        string password = Password;
        SignUpValidationStatus status = await _services.CheckAvailabilityAsync(username, email, password);
        if (status != SignUpValidationStatus.Success)
        {
            Window.ShowMessage(DescribeStatus(status));
            return;
        }

        VerificationSessionDto session = await _services.BeginVerificationAsync(email);
        if (!session.WasSent)
        {
            Window.ShowMessage(UiStrings.VerificationCodeSendErrorMessage);
            return;
        }

        bool isVerified = Window.ShowDialog(Screen.Verification, new VerificationRequest(email, session.SessionId)) == true;
        if (!isVerified) { return; }
        status = await _services.RegisterUserAsync(username, email, password, session.SessionId);
        if (status != SignUpValidationStatus.Success) { Window.ShowMessage(DescribeStatus(status)); }
        else
        {
            Window.ShowMessage(UiStrings.RegistrationSuccessMessage);
            Window.Navigate(Screen.Login, preservePosition: false);
        }
    }

    private string DescribeStatus(SignUpValidationStatus status)
    {
        string statusMessage;
        switch (status)
        {
            case SignUpValidationStatus.EmptyFields:
                statusMessage = UiStrings.RequiredFieldsMessage;
                break;
            case SignUpValidationStatus.InvalidEmail:
                statusMessage = UiStrings.InvalidEmailMessage;
                break;
            case SignUpValidationStatus.PasswordTooShort:
                statusMessage = string.Format(UiStrings.InvalidPasswordMessage, MinimumPasswordLength);
                break;
            case SignUpValidationStatus.UserAlreadyExists:
                statusMessage = UiStrings.MessageUserAlreadyExists;
                break;
            case SignUpValidationStatus.VerificationRequired:
                statusMessage = UiStrings.VerificationCodeExpiredMessage;
                break;
            case SignUpValidationStatus.DatabaseError:
                statusMessage = UiStrings.DatabaseErrorMessage;
                break;
            default:
                statusMessage = UiStrings.DatabaseErrorMessage;
                break;
        }
        return statusMessage;
    }
}
