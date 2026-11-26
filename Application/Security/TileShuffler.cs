using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Server.Application.Security;

public sealed class TileShuffler : ITileShuffler
{
    private const int RandomValueSize = sizeof(uint);
    private const int IndexToCountOffset = 1;

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
            for (int tileIndex = shuffledTiles.Count - IndexToCountOffset;
                tileIndex > CollectionIndex.First;
                tileIndex--)
            {
                generator.GetBytes(randomBytes);
                uint randomValue = BitConverter.ToUInt32(randomBytes, CollectionIndex.First);
                int swapIndex = (int)(randomValue % (uint)(tileIndex + IndexToCountOffset));
                Tile currentTile = shuffledTiles[tileIndex];
                shuffledTiles[tileIndex] = shuffledTiles[swapIndex];
                shuffledTiles[swapIndex] = currentTile;
            }
        }

        return shuffledTiles;
    }
}
