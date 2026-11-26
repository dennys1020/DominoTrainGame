using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Shuffles the tiles before they are dealt.
/// </summary>
public interface ITileShuffler
{
    /// <summary>
    /// Returns the tiles in a random order taken from a cryptographic random number generator.
    /// </summary>
    /// <param name="tiles">The tiles to shuffle. The list is not changed.</param>
    /// <returns>A new list with the same tiles in a random order.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="tiles"/> is null.
    /// </exception>
    IReadOnlyList<Tile> Shuffle(IReadOnlyList<Tile> tiles);
}
