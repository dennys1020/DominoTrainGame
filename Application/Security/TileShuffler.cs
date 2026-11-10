using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.Security;

public sealed class TileShuffler : ITileShuffler
{
    private const int RandomValueSize = sizeof(uint);
    private const int RandomValueOffset = 0;
    private const int FirstTileIndex = 0;

    public IReadOnlyList<Tile> Shuffle(IReadOnlyList<Tile> tiles)
    {
        if (tiles == null)
        {
            throw new ArgumentNullException(nameof(tiles));
        }

        List<Tile> shuffledTiles = new List<Tile>(tiles);
        byte[] randomBytes = new byte[RandomValueSize];

        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            for (int tileIndex = shuffledTiles.Count - 1; tileIndex > FirstTileIndex; tileIndex--)
            {
                generator.GetBytes(randomBytes);
                uint randomValue = BitConverter.ToUInt32(randomBytes, RandomValueOffset);
                int swapIndex = (int)(randomValue % (uint)(tileIndex + 1));
                Tile currentTile = shuffledTiles[tileIndex];
                shuffledTiles[tileIndex] = shuffledTiles[swapIndex];
                shuffledTiles[swapIndex] = currentTile;
            }
        }

        return shuffledTiles;
    }
}
