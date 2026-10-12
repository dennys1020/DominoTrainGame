using System.ComponentModel;

namespace DominoTrainGame.ViewModels;

public abstract class ScreenViewModel : ObservableViewModel
{
    protected readonly IWindowService Window;

    protected ScreenViewModel(IWindowService window)
    {
        Window = window;
        PropertyChangedEventManager.AddHandler(Localization, OnLanguageChanged, string.Empty);
        SettingsCommand = new RelayCommand(() => Window.ShowDialog(Screen.Settings));
        RulesCommand = new RelayCommand(() => Window.ShowDialog(Screen.Rules));
        CloseCommand = new RelayCommand(() => Window.Close());
        CancelCommand = new RelayCommand(() => Window.Close(false));
    }

    public LocalizationViewModel Localization => LocalizationViewModel.Instance;
    public FormattedLocalizationViewModel Uppercase => Localization.Uppercase;
    public FormattedLocalizationViewModel Lowercase => Localization.Lowercase;
    public FormattedLocalizationViewModel WindowTitles => Localization.WindowTitles;
    public string this[string key] => Localization[key];
    public bool IsSpanishSelected { get => Localization.IsSpanishSelected; set => Localization.IsSpanishSelected = value; }
    public bool IsEnglishSelected { get => Localization.IsEnglishSelected; set => Localization.IsEnglishSelected = value; }
    public RelayCommand SettingsCommand { get; }
    public RelayCommand RulesCommand { get; }
    public RelayCommand CloseCommand { get; }
    public RelayCommand CancelCommand { get; }

    private void OnLanguageChanged(object sender, PropertyChangedEventArgs args) => Notify(string.Empty);
}
