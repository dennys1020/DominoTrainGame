using DominoTrainGame.ViewModels.Commands;
using DominoTrainGame.ViewModels.Common;
using DominoTrainGame.ViewModels.Navigation;

namespace DominoTrainGame.ViewModels;

public sealed class DialogViewModel : ScreenViewModel
{
    public DialogViewModel(IWindowService windowService) : base(windowService)
    {
        CancelCommand = new RelayCommand(() => WindowService.Close());
    }

    public RelayCommand CancelCommand { get; }
}
