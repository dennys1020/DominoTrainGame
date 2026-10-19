using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using DominoTrainGame.Resources.Localization;

namespace DominoTrainGame.ViewModels.Localization;

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
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public static LocalizationViewModel Instance { get; }

    public bool IsSpanishSelected
    {
        get
        {
            return !IsEnglishCulture;
        }
        set
        {
            SelectLanguage(value, SpanishCultureName);
        }
    }

    public bool IsEnglishSelected
    {
        get
        {
            return IsEnglishCulture;
        }
        set
        {
            SelectLanguage(value, EnglishCultureName);
        }
    }

    public string this[string key]
    {
        get
        {
            string text = UiStrings.ResourceManager.GetString(key, CurrentCulture) ?? key;
            return text;
        }
    }

    private static CultureInfo CurrentCulture => UiStrings.Culture ?? CultureInfo.CurrentUICulture;

    private static bool IsEnglishCulture => CurrentCulture.TwoLetterISOLanguageName == EnglishLanguageCode;

    private void SelectLanguage(bool isSelected, string cultureName)
    {
        bool isNewLanguage = isSelected && !string.Equals(
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
