using DominoTrainGame.Resources;
using System.ComponentModel;
using System.Globalization;
using DominoTrainGame.Resources.Localization;

namespace DominoTrainGame.ViewModels
{
    public sealed class LocalizationViewModel : INotifyPropertyChanged
    {
        private static readonly LocalizationViewModel _instance = new LocalizationViewModel();

        private CultureInfo _culture;

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
                return UiStrings.ResourceManager.GetString(resourceKey, _culture) ?? string.Empty;
            }
        }

        public bool IsSpanishSelected
        {
            get
            {
                return _culture.Name == SettingsDefaults.SpanishLanguageCode;
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
                return _culture.Name == SettingsDefaults.EnglishLanguageCode;
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
            if (_culture.Name == languageCode)
            {
                return;
            }

            _culture = CultureInfo.GetCultureInfo(languageCode);
            UiStrings.Culture = _culture;

            // An empty property name refreshes every translated label and language selection.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }
}
