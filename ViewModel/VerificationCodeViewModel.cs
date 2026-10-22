using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

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
