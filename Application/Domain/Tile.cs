using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class Tile
{
    public const int MaximumPips = 12;
    private const int MinimumPips = 0;
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

    public static IReadOnlyList<Tile> CreateDoubleTwelveSet()
    {
        List<Tile> tiles = new List<Tile>();

        for (int leftPips = MinimumPips; leftPips <= MaximumPips; leftPips++)
        {
            for (int rightPips = leftPips; rightPips <= MaximumPips; rightPips++)
            {
                tiles.Add(new Tile(leftPips, rightPips));
            }
        }

        return tiles;
    }

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
}
