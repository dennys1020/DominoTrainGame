using DominoTrainGame.ViewModels.Commands;
using DominoTrainGame.ViewModels.Common;
using DominoTrainGame.ViewModels.Navigation;

namespace DominoTrainGame.ViewModels;

public sealed class VerificationCodeViewModel : ScreenViewModel
{
    public VerificationCodeViewModel(IWindowService windowService) : base(windowService)
    {
        VerifyCommand = new RelayCommand(() => WindowService.Navigate(Screen.Login));
        CancelCommand = new RelayCommand(() => WindowService.Navigate(Screen.SignUp));
    }

    public RelayCommand VerifyCommand { get; }

    public RelayCommand CancelCommand { get; }
}
