using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using DominoTrainGame.Client.Models.Resources.Localization;
using DominoTrainGame.Client.ViewModel.Commands;

namespace DominoTrainGame.Client.ViewModel.Localization;

public sealed class LocalizationViewModel : INotifyPropertyChanged
{
    private const string IndexerPropertyName = "Item[]";
    private const string SpanishCultureName = "es-MX";
    private const string EnglishCultureName = "en-US";
    private const string EnglishLanguageCode = "en";

    static LocalizationViewModel()
    {
        Instance = new LocalizationViewModel();
    }

    private LocalizationViewModel()
    {
        SelectSpanishCommand = new RelayCommand(() => SelectLanguage(SpanishCultureName));
        SelectEnglishCommand = new RelayCommand(() => SelectLanguage(EnglishCultureName));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public static LocalizationViewModel Instance { get; }

    public bool IsSpanishSelected => !IsEnglishCulture;

    public bool IsEnglishSelected => IsEnglishCulture;

    public RelayCommand SelectSpanishCommand { get; }

    public RelayCommand SelectEnglishCommand { get; }

    public string this[string key]
    {
        get
        {
            string text = UiStrings.ResourceManager.GetString(key, CurrentCulture) ?? key;
            return text;
        }
    }

    private static CultureInfo CurrentCulture => UiStrings.Culture ?? CultureInfo.CurrentUICulture;

    private static bool IsEnglishCulture => string.Equals(
        CurrentCulture.TwoLetterISOLanguageName,
        EnglishLanguageCode,
        StringComparison.OrdinalIgnoreCase);

    private void SelectLanguage(string cultureName)
    {
        bool isNewLanguage = !string.Equals(
            CurrentCulture.Name,
            cultureName,
            StringComparison.OrdinalIgnoreCase);

        if (isNewLanguage)
        {
            ChangeCulture(cultureName);
        }
    }

    private void ChangeCulture(string cultureName)
    {
        CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);

        UiStrings.Culture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        OnPropertyChanged(IndexerPropertyName);
        OnPropertyChanged(nameof(IsSpanishSelected));
        OnPropertyChanged(nameof(IsEnglishSelected));
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
