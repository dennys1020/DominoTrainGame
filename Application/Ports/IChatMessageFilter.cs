namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Censors the offensive words of the chat messages.
/// </summary>
public interface IChatMessageFilter
{
    /// <summary>
    /// Replaces every character of each forbidden word with a censor character.
    /// </summary>
    /// <remarks>
    /// Accents, look-alike characters and repeated letters are ignored when a word is compared with the
    /// forbidden list, so the filter also catches common spelling tricks.
    /// </remarks>
    /// <param name="text">The normalized text of the message.</param>
    /// <returns>
    /// The censored text with the same length as <paramref name="text"/>, or the same text when it has no
    /// forbidden words.
    /// </returns>
    string Filter(string text);
}
