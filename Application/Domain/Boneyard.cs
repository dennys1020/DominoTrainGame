using System;
using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class Boneyard
{
    private const int NoTiles = 0;

    private readonly Queue<Tile> _tiles;

    public Boneyard(IEnumerable<Tile> tiles)
    {
        _tiles = new Queue<Tile>(tiles ?? throw new ArgumentNullException(nameof(tiles)));
    }

    public int Count => _tiles.Count;

    public bool HasTiles => Count > NoTiles;

    public bool TryDraw(out Tile tile)
    {
        bool hasTiles = HasTiles;
        tile = hasTiles ? _tiles.Dequeue() : Tile.Empty;
        return hasTiles;
    }
}
