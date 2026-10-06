using System.ComponentModel;
using DominoTrainGame.Converters;

namespace DominoTrainGame.ViewModels;

public sealed class FormattedLocalizationViewModel : INotifyPropertyChanged
{
    private readonly LocalizationViewModel _localization;
    private readonly LocalizedTextConverter _converter;

    public FormattedLocalizationViewModel(
        LocalizationViewModel localization,
        LocalizedTextConverter converter)
    {
        _localization = localization;
        _converter = converter;
        _localization.PropertyChanged += OnLanguageChanged;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string this[string resourceKey]
    {
        get
        {
            string resourceValue = _localization[resourceKey];
            string formattedValue = _converter.Convert(resourceValue, _localization.Culture);
            return formattedValue;
        }
    }

    private void OnLanguageChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }
}
