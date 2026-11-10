using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class HandUpdatedNotice
{
    public HandUpdatedNotice(int recipientPlayerId, IReadOnlyList<Tile> tiles)
    {
        RecipientPlayerId = recipientPlayerId;
        Tiles = new List<Tile>(tiles ?? throw new ArgumentNullException(nameof(tiles)));
    }

    public int RecipientPlayerId { get; }

    public IReadOnlyList<Tile> Tiles { get; }
}
