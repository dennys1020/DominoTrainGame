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

    private static readonly char[] _joinerCharacters;
    private static readonly Dictionary<char, char> _lookAlikeCharacters;

    static ChatTextNormalizer()
    {
        _joinerCharacters = new[] { '.', '-', '_', '*' };
        _lookAlikeCharacters = new Dictionary<char, char>
        {
            { '0', 'o' },
            { '1', 'i' },
            { '3', 'e' },
            { '4', 'a' },
            { '5', 's' },
            { '7', 't' },
            { '@', 'a' },
            { '$', 's' }
        };
    }

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
            if (!IsAccentMark(character) && !IsJoiner(character))
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
        string tokenWithoutLeadingJoiners = rawToken.TrimStart(_joinerCharacters);
        string tokenText = tokenWithoutLeadingJoiners.TrimEnd(_joinerCharacters);
        int trimmedStart = tokenStart + rawToken.Length - tokenWithoutLeadingJoiners.Length;

        if (tokenText.Length > EmptyLength)
        {
            tokens.Add(new ChatToken(trimmedStart, tokenText));
        }

        currentToken.Clear();
    }

    private static bool IsWordCharacter(char character)
    {
        bool isLetterOrDigit = char.IsLetterOrDigit(character);
        bool isSymbol = _lookAlikeCharacters.ContainsKey(character) || IsJoiner(character);
        bool isWordCharacter = isLetterOrDigit || IsAccentMark(character) || isSymbol;
        return isWordCharacter;
    }

    private static bool IsAccentMark(char character)
    {
        bool isAccentMark = CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark;
        return isAccentMark;
    }

    private static bool IsJoiner(char character)
    {
        bool isJoiner = Array.IndexOf(_joinerCharacters, character) > NotFoundIndex;
        return isJoiner;
    }

    private static char ReplaceLookAlike(char character)
    {
        char replacement = _lookAlikeCharacters.ContainsKey(character) ? _lookAlikeCharacters[character] : character;
        return replacement;
    }
}
