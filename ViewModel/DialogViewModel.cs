using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;

namespace DominoTrainGame.Client.ViewModel;

public sealed class DialogViewModel : ScreenViewModel
{
    public DialogViewModel(IWindowService windowService) : base(windowService)
    {
        CancelCommand = new RelayCommand(() => WindowService.Close());
    }

    public RelayCommand CancelCommand { get; }
}
