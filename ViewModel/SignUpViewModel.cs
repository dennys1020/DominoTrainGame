using DominoTrainGame.ViewModels.Commands;
using DominoTrainGame.ViewModels.Common;
using DominoTrainGame.ViewModels.Navigation;

namespace DominoTrainGame.ViewModels;

public sealed class SignUpViewModel : ScreenViewModel
{
    public SignUpViewModel(IWindowService windowService) : base(windowService)
    {
        LoginCommand = new RelayCommand(() => WindowService.Navigate(Screen.Login));
        SignUpCommand = new RelayCommand(() => WindowService.Navigate(Screen.Verification));
    }

    public RelayCommand LoginCommand { get; }

    public RelayCommand SignUpCommand { get; }
}
