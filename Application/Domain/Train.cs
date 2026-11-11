using System;
using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class Train
{
    public const int MexicanTrainOwnerId = 0;
    private const int EmptyValue = -1;

    private readonly List<Tile> _tiles;

    static Train()
    {
        Empty = new Train(EmptyValue, EmptyValue);
    }

    public Train(int ownerId, int enginePips)
    {
        _tiles = new List<Tile>();
        OwnerId = ownerId;
        EndPips = enginePips;
        IsOpen = IsMexicanTrain;
    }

    public static Train Empty { get; }

    public int OwnerId { get; }

    public int EndPips { get; private set; }

    public bool IsOpen { get; private set; }

    public bool IsMexicanTrain => OwnerId == MexicanTrainOwnerId;

    public IReadOnlyList<Tile> Tiles => _tiles;

    public bool CanAccept(Tile tile)
    {
        if (tile == null)
        {
            throw new ArgumentNullException(nameof(tile));
        }

        bool canAccept = tile.HasPips(EndPips);
        return canAccept;
    }

    public bool CanBePlayedBy(int playerId)
    {
        bool isOwner = OwnerId == playerId;
        bool canBePlayed = isOwner || IsOpen;
        return canBePlayed;
    }

    public void AddTile(Tile tile)
    {
        if (!CanAccept(tile))
        {
            throw new InvalidOperationException("The tile does not match the end of the train.");
        }

        EndPips = tile.GetOppositePips(EndPips);
        _tiles.Add(tile);
    }

    public void Open()
    {
        IsOpen = true;
    }

    public void Close()
    {
        IsOpen = IsMexicanTrain;
    }
}
