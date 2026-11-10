using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class TrainSnapshot
{
    public TrainSnapshot(int ownerId, bool isOpen, IReadOnlyList<Tile> tiles)
    {
        OwnerId = ownerId;
        IsOpen = isOpen;
        Tiles = new List<Tile>(tiles ?? throw new ArgumentNullException(nameof(tiles)));
    }

    public int OwnerId { get; }

    public bool IsOpen { get; }

    public IReadOnlyList<Tile> Tiles { get; }
}
