using System;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel.Common;

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
