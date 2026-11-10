using System;
using System.Collections.Generic;
using System.Windows.Input;
using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel;

public sealed class TrainViewModel
{
    public TrainViewModel(TrainSummary train, bool canReceiveTile, ICommand dropTileCommand)
    {
        if (train == null)
        {
            throw new ArgumentNullException(nameof(train));
        }

        TrainId = train.TrainId;
        OwnerUsername = train.Owner.Username;
        IsMexicanTrain = train.IsMexicanTrain;
        IsOpen = train.IsOpen;
        Tiles = train.Tiles;
        IsAvailable = canReceiveTile;
        DropTileCommand = dropTileCommand ?? throw new ArgumentNullException(nameof(dropTileCommand));
    }

    public int TrainId { get; }

    public string OwnerUsername { get; }

    public bool IsMexicanTrain { get; }

    public bool IsOpen { get; }

    public bool IsAvailable { get; }

    public IReadOnlyList<TileSummary> Tiles { get; }

    public ICommand DropTileCommand { get; }
}
