using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Resources.Localization;
using DominoTrainGame.Client.Models.DominoTrainService;

namespace DominoTrainGame.ViewModels;

public sealed class SignUpViewModel : ScreenViewModel
{
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
        User user = new User
        {
            Username = Username,
            Email = Email.Trim(),
            Password = Password,
            Language = Localization.Culture.TwoLetterISOLanguageName
        };

        bool isRegistered = await _services.RegisterUserAsync(user);

        if (Window.IsVisible)
        {
            if (isRegistered)
            {
                Window.ShowMessage(UiStrings.RegistrationSuccessMessage);
                Window.Navigate(Screen.Login, preservePosition: false);
            }
            else
            {
                Window.ShowMessage(UiStrings.RegistrationFailedMessage);
            }
        }
    }
}
