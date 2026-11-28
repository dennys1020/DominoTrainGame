using System;
using System.Text.RegularExpressions;

namespace DominoTrainGame.Shared.Common;

public static class InputText
{
    public const int EmptyLength = 0;

    private const int RegexTimeoutMilliseconds = 100;

    public static string Normalize(string text)
    {
        string normalizedText = text?.Trim() ?? string.Empty;
        return normalizedText;
    }

    public static Regex CreateRegex(string pattern)
    {
        TimeSpan matchTimeout = TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds);
        Regex regex = new Regex(pattern, RegexOptions.CultureInvariant, matchTimeout);
        return regex;
    }
}
