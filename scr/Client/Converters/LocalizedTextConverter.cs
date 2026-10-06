using System.Globalization;
using System.Windows.Controls;
using DominoTrainGame.Resources.Localization;

namespace DominoTrainGame.Converters;

public sealed class LocalizedTextConverter
{
    public CharacterCasing Casing
    {
        get;
        set;
    }

    public bool IncludeGameTitle
    {
        get;
        set;
    }

    public string Convert(string value, CultureInfo culture)
    {
        string text = value ?? string.Empty;
        CultureInfo textCulture = culture ?? CultureInfo.CurrentUICulture;

        switch (Casing)
        {
            case CharacterCasing.Upper:
                text = text.ToUpper(textCulture);
                break;
            case CharacterCasing.Lower:
                text = text.ToLower(textCulture);
                break;
        }

        if (IncludeGameTitle)
        {
            object[] titleValues = { UiStrings.GameTitle, text };
            text = string.Format(textCulture, UiStrings.WindowTitleFormat, titleValues);
        }

        return text;
    }
}
