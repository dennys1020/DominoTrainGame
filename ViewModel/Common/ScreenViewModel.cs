using System;
using DominoTrainGame.ViewModels.Commands;
using DominoTrainGame.ViewModels.Navigation;

namespace DominoTrainGame.ViewModels.Common;

public abstract class ScreenViewModel : ObservableViewModel
{
    protected ScreenViewModel(IWindowService windowService)
    {
        WindowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        SettingsCommand = new RelayCommand(() => WindowService.ShowDialog(Screen.Settings));
        RulesCommand = new RelayCommand(() => WindowService.ShowDialog(Screen.Rules));
        CloseCommand = new RelayCommand(() => WindowService.Close());
    }

    public RelayCommand SettingsCommand { get; }

    public RelayCommand RulesCommand { get; }

    public RelayCommand CloseCommand { get; }

    protected IWindowService WindowService { get; }
}
