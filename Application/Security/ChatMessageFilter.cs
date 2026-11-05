using System;
using System.Collections.Generic;
using System.Text;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.Security;

public sealed class ChatMessageFilter : IChatMessageFilter
{
    private const char CensorCharacter = '#';
    private const char PrefixMarker = '*';
    private const int SingleRepetition = 1;
    private const int DoubleRepetition = 2;
    private const int EmptyLength = 0;

    private readonly HashSet<string> _forbiddenWords = new HashSet<string>(StringComparer.Ordinal);
    private readonly List<string> _forbiddenPrefixes = new List<string>();

    public ChatMessageFilter(IForbiddenWordSource forbiddenWordSource)
    {
        if (forbiddenWordSource == null)
        {
            throw new ArgumentNullException(nameof(forbiddenWordSource));
        }

        foreach (string forbiddenEntry in forbiddenWordSource.LoadEntries())
        {
            AddForbiddenEntry(forbiddenEntry);
        }
    }

    public string Filter(string text)
    {
        string originalText = text ?? string.Empty;
        StringBuilder filteredText = new StringBuilder(originalText);

        foreach (ChatToken token in ChatTextNormalizer.Tokenize(originalText))
        {
            if (IsForbidden(token.Text))
            {
                CensorToken(filteredText, token);
            }
        }

        string censoredText = filteredText.ToString();
        return censoredText;
    }

    private static void CensorToken(StringBuilder text, ChatToken token)
    {
        int tokenEnd = token.Start + token.Text.Length;

        for (int index = token.Start; index < tokenEnd; index++)
        {
            text[index] = CensorCharacter;
        }
    }

    private void AddForbiddenEntry(string forbiddenEntry)
    {
        string trimmedEntry = forbiddenEntry?.Trim() ?? string.Empty;
        bool isPrefix = trimmedEntry.EndsWith(PrefixMarker.ToString(), StringComparison.Ordinal);
        string normalizedEntry = ChatTextNormalizer.Normalize(trimmedEntry);

        if (normalizedEntry.Length > EmptyLength)
        {
            if (isPrefix)
            {
                _forbiddenPrefixes.Add(normalizedEntry);
            }
            else
            {
                _forbiddenWords.Add(normalizedEntry);
            }
        }
    }

    private bool IsForbidden(string word)
    {
        string normalizedWord = ChatTextNormalizer.Normalize(word);
        string wordWithDoubleLetters = ChatTextNormalizer.LimitRepetitions(normalizedWord, DoubleRepetition);
        string wordWithSingleLetters = ChatTextNormalizer.LimitRepetitions(normalizedWord, SingleRepetition);

        bool isForbidden = MatchesForbiddenEntry(normalizedWord)
            || MatchesForbiddenEntry(wordWithDoubleLetters)
            || MatchesForbiddenEntry(wordWithSingleLetters);
        return isForbidden;
    }

    private bool MatchesForbiddenEntry(string normalizedWord)
    {
        bool isExactMatch = _forbiddenWords.Contains(normalizedWord);
        bool isPrefixMatch = _forbiddenPrefixes.Exists(
            prefix => normalizedWord.StartsWith(prefix, StringComparison.Ordinal));
        return isExactMatch || isPrefixMatch;
    }
}
