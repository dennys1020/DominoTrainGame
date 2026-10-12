using System.Threading.Tasks;
using DominoTrainGame.Contracts;
using DominoTrainGame.Resources.Localization;

namespace DominoTrainGame.ViewModels;

public sealed class RecoverPasswordViewModel : ScreenViewModel
{
    private const int MinimumPasswordLength = 12;
    private readonly IGameServices _services;
    private int _playerId;
    private string _identifier = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmation = string.Empty;
    private string _message = string.Empty;
    private bool _isPasswordStep;

    public RecoverPasswordViewModel(IGameServices services, IWindowService window) : base(window)
    {
        _services = services;
        SendCodeCommand = new AsyncCommand(SendCodeAsync);
        UpdatePasswordCommand = new RelayCommand(UpdatePassword);
    }

    public string Identifier { get => _identifier; set => SetField(ref _identifier, value); }
    public string NewPassword { get => _newPassword; set => SetField(ref _newPassword, value); }
    public string Confirmation { get => _confirmation; set => SetField(ref _confirmation, value); }
    public string Message { get => _message; private set => SetField(ref _message, value); }
    public bool IsPasswordStep { get => _isPasswordStep; private set { SetField(ref _isPasswordStep, value); Notify(nameof(IsIdentifyStep)); } }
    public bool IsIdentifyStep => !IsPasswordStep;
    public AsyncCommand SendCodeCommand { get; }
    public RelayCommand UpdatePasswordCommand { get; }

    private async Task SendCodeAsync()
    {
        RecoverPasswordValidationStatus status = _services.FindAccount(Identifier, out int playerId, out string email);
        Message = DescribeStatus(status);
        if (status != RecoverPasswordValidationStatus.Success) { return; }
        _playerId = playerId;
        IVerificationSession session = _services.CreateVerificationSession();
        string code = session.GenerateCode();
        bool wasSent = await _services.SendVerificationCodeAsync(email, code);
        if (wasSent)
        {
            if (Window.ShowDialog(Screen.Verification, new VerificationRequest(email, session)) == true)
            {
                IsPasswordStep = true;
                Message = string.Empty;
            }
        }
        else { Message = UiStrings.VerificationCodeSendErrorMessage; }
    }

    private void UpdatePassword()
    {
        RecoverPasswordValidationStatus status = _services.UpdatePassword(_playerId, NewPassword, Confirmation);
        Message = DescribeStatus(status);
        if (status == RecoverPasswordValidationStatus.Success) { Window.Close(true); }
    }

    private static string DescribeStatus(RecoverPasswordValidationStatus status)
    {
        string message;

        switch (status)
        {
            case RecoverPasswordValidationStatus.Success:
                message = UiStrings.RecoverPasswordSuccessMessage;
                break;
            case RecoverPasswordValidationStatus.UserNotFound:
                message = UiStrings.MessageUserNotFound;
                break;
            case RecoverPasswordValidationStatus.PasswordTooShort:
                message = string.Format(
                    UiStrings.InvalidPasswordMessage,
                    MinimumPasswordLength
                );
                break;
            case RecoverPasswordValidationStatus.PasswordsDoNotMatch:
                message = UiStrings.PasswordsDoNotMatchMessage;
                break;
            case RecoverPasswordValidationStatus.DatabaseError:
                message = UiStrings.DatabaseErrorMessage;
                break;
            default:
                message = UiStrings.RequiredFieldsMessage;
                break;
        }

        return message;
    }
}
