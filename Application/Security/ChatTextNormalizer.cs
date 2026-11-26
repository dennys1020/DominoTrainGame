using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Server.Application.Security;

internal static class ChatTextNormalizer
{
    private const int FirstRepetition = 1;

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
        List<ChatToken> tokens = new List<ChatToken>();
        StringBuilder currentToken = new StringBuilder();
        int tokenStart = CollectionIndex.First;

        for (int index = CollectionIndex.First; index < text.Length; index++)
        {
            char character = text[index];

            if (IsWordCharacter(character))
            {
                tokenStart = currentToken.Length == InputText.EmptyLength ? index : tokenStart;
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
        string decomposedWord = word.ToLowerInvariant().Normalize(NormalizationForm.FormD);
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
        char previousCharacter = char.MinValue;

        for (int index = CollectionIndex.First; index < word.Length; index++)
        {
            char character = word[index];
            bool isRepeated = index > CollectionIndex.First && character == previousCharacter;

            if (isRepeated)
            {
                repetitions++;
            }
            else
            {
                repetitions = FirstRepetition;
            }

            if (repetitions <= maximumRepetitions)
            {
                limitedWord.Append(character);
            }

            previousCharacter = character;
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

        if (tokenText.Length > InputText.EmptyLength)
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
        bool isJoiner = CollectionIndex.IsFound(Array.IndexOf(_joinerCharacters, character));
        return isJoiner;
    }

    private static char ReplaceLookAlike(char character)
    {
        char replacement = _lookAlikeCharacters.ContainsKey(character) ? _lookAlikeCharacters[character] : character;
        return replacement;
    }
}
