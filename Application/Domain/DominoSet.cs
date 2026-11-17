using System;
using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class DominoSet
{
    public const int MinimumPlayerCount = 2;
    private const int LastHandSizeOffset = 1;
    private const int DoubleSixPips = 6;
    private const int DoubleNinePips = 9;
    private const int DoubleTwelvePips = 12;
    private const int StandardHandSize = 15;
    private const int DoubleNineThreePlayerHandSize = 13;
    private const int DoubleNineFourPlayerHandSize = 10;
    private const int DoubleSixTwoPlayerHandSize = 7;
    private const int DoubleSixGroupHandSize = 5;

    private static readonly Dictionary<int, DominoSet> _setsByHighestDouble;

    private readonly List<int> _handSizesFromMinimumPlayers;

    static DominoSet()
    {
        DoubleSix = new DominoSet(
            DoubleSixPips,
            new[] { DoubleSixTwoPlayerHandSize, DoubleSixGroupHandSize, DoubleSixGroupHandSize });
        DoubleNine = new DominoSet(
            DoubleNinePips,
            new[] { StandardHandSize, DoubleNineThreePlayerHandSize, DoubleNineFourPlayerHandSize });
        DoubleTwelve = new DominoSet(
            DoubleTwelvePips,
            new[] { StandardHandSize, StandardHandSize, StandardHandSize });
        _setsByHighestDouble = new Dictionary<int, DominoSet>
        {
            { DoubleSixPips, DoubleSix },
            { DoubleNinePips, DoubleNine },
            { DoubleTwelvePips, DoubleTwelve }
        };
    }

    private DominoSet(int highestDouble, IReadOnlyList<int> handSizesFromMinimumPlayers)
    {
        HighestDouble = highestDouble;
        _handSizesFromMinimumPlayers = new List<int>(handSizesFromMinimumPlayers);
    }

    public static DominoSet DoubleSix { get; }

    public static DominoSet DoubleNine { get; }

    public static DominoSet DoubleTwelve { get; }

    public int HighestDouble { get; }

    public int MaximumPlayerCount => MinimumPlayerCount + _handSizesFromMinimumPlayers.Count - LastHandSizeOffset;

    public static DominoSet FromHighestDouble(int highestDouble)
    {
        if (!_setsByHighestDouble.ContainsKey(highestDouble))
        {
            throw new ArgumentOutOfRangeException(
                nameof(highestDouble),
                "The highest double must be six, nine or twelve.");
        }

        DominoSet dominoSet = _setsByHighestDouble[highestDouble];
        return dominoSet;
    }

    public bool SupportsPlayerCount(int playerCount)
    {
        bool isSupported = playerCount >= MinimumPlayerCount && playerCount <= MaximumPlayerCount;
        return isSupported;
    }

    public int GetHandSize(int playerCount)
    {
        if (!SupportsPlayerCount(playerCount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(playerCount),
                "The domino set has no hand size for that number of players.");
        }

        int handSize = _handSizesFromMinimumPlayers[playerCount - MinimumPlayerCount];
        return handSize;
    }

    public IReadOnlyList<Tile> CreateTiles()
    {
        List<Tile> tiles = new List<Tile>();

        for (int leftPips = Tile.MinimumPips; leftPips <= HighestDouble; leftPips++)
        {
            for (int rightPips = leftPips; rightPips <= HighestDouble; rightPips++)
            {
                tiles.Add(new Tile(leftPips, rightPips));
            }
        }

        return tiles;
    }
}
