using System;

namespace DominoTrainGame.Shared.Common;

public static class GameLanguage
{
    public const string EnglishCode = "en";

    public static bool IsEnglish(string languageName)
    {
        bool isEnglish = InputText.Normalize(languageName).StartsWith(EnglishCode, StringComparison.OrdinalIgnoreCase);
        return isEnglish;
    }
}
