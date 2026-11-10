using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class Tile
{
    public const int MaximumPips = 12;
    private const int MinimumPips = 0;

    public Tile(int leftPips, int rightPips)
    {
        LeftPips = leftPips;
        RightPips = rightPips;
    }

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
}
