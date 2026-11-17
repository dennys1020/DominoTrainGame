using System;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class Tile
{
    public const int MinimumPips = 0;
    private const int EmptyPips = -1;

    static Tile()
    {
        Empty = new Tile(EmptyPips, EmptyPips);
    }

    public Tile(int leftPips, int rightPips)
    {
        LeftPips = leftPips;
        RightPips = rightPips;
    }

    public static Tile Empty { get; }

    public int LeftPips { get; }

    public int RightPips { get; }

    public bool IsDouble => LeftPips == RightPips;

    public int PipCount => LeftPips + RightPips;

    public bool HasPips(int pips)
    {
        bool hasPips = LeftPips == pips || RightPips == pips;
        return hasPips;
    }

    public int GetOppositePips(int pips)
    {
        int oppositePips = LeftPips == pips ? RightPips : LeftPips;
        return oppositePips;
    }

    public bool IsSameTile(Tile otherTile)
    {
        if (otherTile == null)
        {
            throw new ArgumentNullException(nameof(otherTile));
        }

        bool hasSameOrientation = LeftPips == otherTile.LeftPips && RightPips == otherTile.RightPips;
        bool hasInvertedOrientation = LeftPips == otherTile.RightPips && RightPips == otherTile.LeftPips;
        bool isSameTile = hasSameOrientation || hasInvertedOrientation;
        return isSameTile;
    }
}
