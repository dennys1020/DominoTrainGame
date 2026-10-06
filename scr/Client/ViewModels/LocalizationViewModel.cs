using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using DominoTrainGame.Converters;
using DominoTrainGame.Resources;
using DominoTrainGame.Resources.Localization;

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
        Uppercase = new FormattedLocalizationViewModel(this, new LocalizedTextConverter
        {
            Casing = CharacterCasing.Upper
        });
        Lowercase = new FormattedLocalizationViewModel(this, new LocalizedTextConverter
        {
            Casing = CharacterCasing.Lower
        });
        WindowTitles = new FormattedLocalizationViewModel(this, new LocalizedTextConverter
        {
            IncludeGameTitle = true
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public static LocalizationViewModel Instance
    {
        get
        {
            return _instance;
        }
    }

    public CultureInfo Culture
    {
        get
        {
            return _culture;
        }
    }

    public FormattedLocalizationViewModel Uppercase
    {
        get;
    }

    public FormattedLocalizationViewModel Lowercase
    {
        get;
    }

    public FormattedLocalizationViewModel WindowTitles
    {
        get;
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
