using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class TrainSummary
{
    public TrainSummary(int trainId, PlayerSummary owner, TrainStatus status)
    {
        TrainId = trainId;
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        Status = status;
        Tiles = new List<TileSummary>().AsReadOnly();
    }

    private TrainSummary(TrainSummary train, IReadOnlyList<TileSummary> tiles)
    {
        TrainId = train.TrainId;
        Owner = train.Owner;
        Status = train.Status;
        Tiles = new List<TileSummary>(tiles).AsReadOnly();
    }

    public int TrainId { get; }

    public PlayerSummary Owner { get; }

    public TrainStatus Status { get; }

    public bool IsMexicanTrain => Status == TrainStatus.Mexican;

    public bool IsOpen => Status == TrainStatus.Open;

    public IReadOnlyList<TileSummary> Tiles { get; }

    public TrainSummary WithTiles(IReadOnlyList<TileSummary> tiles)
    {
        if (tiles == null)
        {
            throw new ArgumentNullException(nameof(tiles));
        }

        TrainSummary train = new TrainSummary(this, tiles);
        return train;
    }
}
