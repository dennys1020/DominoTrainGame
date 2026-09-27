using DominoTrainGame.Resources;
using DominoTrainGame.Resources.Localization;
using System;
using System.ComponentModel;
using System.Globalization;

namespace DominoTrainGame.ViewModels;

public sealed class LocalizationViewModel : INotifyPropertyChanged
{
    private static readonly LocalizationViewModel _instance;

    private CultureInfo _culture;

    static LocalizationViewModel()
    {
        _instance = new LocalizationViewModel();
    }

    private LocalizationViewModel()
    {
        _culture = CultureInfo.GetCultureInfo(SettingsDefaults.DefaultLanguageCode);
        UiStrings.Culture = _culture;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public static LocalizationViewModel Instance
    {
        get
        {
            return _instance;
        }
    }

    public string this[string resourceKey]
    {
        get
        {
            string resourceValue = UiStrings.ResourceManager.GetString(resourceKey, _culture) ?? string.Empty;
            return resourceValue;
        }
    }

    public bool IsSpanishSelected
    {
        get
        {
            bool isSpanishSelected = _culture.Name == SettingsDefaults.SpanishLanguageCode;
            return isSpanishSelected;
        }
        set
        {
            if (value)
            {
                ChangeLanguage(SettingsDefaults.SpanishLanguageCode);
            }
        }
    }

    public bool IsEnglishSelected
    {
        get
        {
            bool isEnglishSelected = _culture.Name == SettingsDefaults.EnglishLanguageCode;
            return isEnglishSelected;
        }
        set
        {
            if (value)
            {
                ChangeLanguage(SettingsDefaults.EnglishLanguageCode);
            }
        }
    }

    private void ChangeLanguage(string languageCode)
    {
        if (_culture.Name != languageCode)
        {
            _culture = CultureInfo.GetCultureInfo(languageCode);
            UiStrings.Culture = _culture;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }
}
