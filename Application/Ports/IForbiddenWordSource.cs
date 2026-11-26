using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Loads the list of words that are not allowed in the chat.
/// </summary>
public interface IForbiddenWordSource
{
    /// <summary>
    /// Loads every entry of the forbidden word lists.
    /// </summary>
    /// <returns>
    /// One entry per line of the lists. An entry that ends with <c>*</c> forbids every word that starts with it.
    /// </returns>
    IReadOnlyList<string> LoadEntries();
}
