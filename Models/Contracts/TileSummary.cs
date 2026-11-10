namespace DominoTrainGame.Client.Models.Contracts;

public sealed class TileSummary
{
    public TileSummary(int tileId, int leftPips, int rightPips)
    {
        TileId = tileId;
        LeftPips = leftPips;
        RightPips = rightPips;
    }

    public int TileId { get; }

    public int LeftPips { get; }

    public int RightPips { get; }

    public bool IsDouble => LeftPips == RightPips;
}
