using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DominoTrainGame.Server.Application.Security;

internal static class ChatTextNormalizer
{
    private const int EmptyLength = 0;
    private const int FirstRepetition = 1;
    private const int RepetitionStep = 1;
    private const int NotFoundIndex = -1;

    private static readonly char[] JoinerCharacters = { '.', '-', '_', '*' };

    private static readonly Dictionary<char, char> LookAlikeCharacters = new Dictionary<char, char>
    {
        { '0', 'o' },
        { '1', 'i' },
        { '3', 'e' },
        { '4', 'a' },
        { '5', 's' },
        { '7', 't' },
        { '@', 'a' },
        { '$', 's' },
    };

    public static List<ChatToken> Tokenize(string text)
    {
        string sourceText = text ?? string.Empty;
        List<ChatToken> tokens = new List<ChatToken>();
        StringBuilder currentToken = new StringBuilder();
        int tokenStart = EmptyLength;

        for (int index = EmptyLength; index < sourceText.Length; index++)
        {
            char character = sourceText[index];

            if (IsWordCharacter(character))
            {
                tokenStart = currentToken.Length == EmptyLength ? index : tokenStart;
                currentToken.Append(character);
            }
            else
            {
                AddToken(tokens, currentToken, tokenStart);
            }
        }

        AddToken(tokens, currentToken, tokenStart);
        return tokens;
    }

    public static string Normalize(string word)
    {
        string decomposedWord = (word ?? string.Empty).ToLowerInvariant().Normalize(NormalizationForm.FormD);
        StringBuilder normalizedWord = new StringBuilder(decomposedWord.Length);

        foreach (char character in decomposedWord)
        {
            bool isAccentMark = CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark;

            if (!isAccentMark && !IsJoiner(character))
            {
                normalizedWord.Append(ReplaceLookAlike(character));
            }
        }

        string normalizedText = normalizedWord.ToString();
        return normalizedText;
    }

    public static string LimitRepetitions(string word, int maximumRepetitions)
    {
        StringBuilder limitedWord = new StringBuilder(word.Length);
        int repetitions = FirstRepetition;

        for (int index = EmptyLength; index < word.Length; index++)
        {
            bool isRepeated = index > EmptyLength && word[index] == word[index - RepetitionStep];
            repetitions = isRepeated ? repetitions + RepetitionStep : FirstRepetition;

            if (repetitions <= maximumRepetitions)
            {
                limitedWord.Append(word[index]);
            }
        }

        string limitedText = limitedWord.ToString();
        return limitedText;
    }

    private static void AddToken(List<ChatToken> tokens, StringBuilder currentToken, int tokenStart)
    {
        string rawToken = currentToken.ToString();
        string tokenWithoutLeadingJoiners = rawToken.TrimStart(JoinerCharacters);
        string tokenText = tokenWithoutLeadingJoiners.TrimEnd(JoinerCharacters);
        int trimmedStart = tokenStart + rawToken.Length - tokenWithoutLeadingJoiners.Length;

        if (tokenText.Length > EmptyLength)
        {
            tokens.Add(new ChatToken(trimmedStart, tokenText));
        }

        currentToken.Clear();
    }

    private static bool IsWordCharacter(char character)
    {
        bool isWordCharacter = char.IsLetterOrDigit(character)
            || CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark
            || LookAlikeCharacters.ContainsKey(character)
            || IsJoiner(character);
        return isWordCharacter;
    }

    private static bool IsJoiner(char character)
    {
        bool isJoiner = Array.IndexOf(JoinerCharacters, character) > NotFoundIndex;
        return isJoiner;
    }

    private static char ReplaceLookAlike(char character)
    {
        char replacement = LookAlikeCharacters.TryGetValue(character, out char letter) ? letter : character;
        return replacement;
    }
}
