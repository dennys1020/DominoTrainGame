using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class PlayerHand
{
    static PlayerHand()
    {
        Empty = new PlayerHand(0, new List<TileSummary>());
    }

    public PlayerHand(long version, IReadOnlyList<TileSummary> tiles)
    {
        if (tiles == null)
        {
            throw new ArgumentNullException(nameof(tiles));
        }

        Version = version;
        Tiles = new List<TileSummary>(tiles).AsReadOnly();
    }

    public static PlayerHand Empty { get; }

    public long Version { get; }

    public IReadOnlyList<TileSummary> Tiles { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);
}
