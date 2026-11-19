using System;
using System.Collections.Generic;
using System.Windows.Input;
using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel;

public sealed class TrainViewModel
{
    private const int MaxVisibleTiles = 6;
    private const int NoHiddenTiles = 0;

    public TrainViewModel(TrainSummary train, bool isLocalPlayerTrain, bool canReceiveTile, ICommand dropTileCommand)
    {
        if (train == null)
        {
            throw new ArgumentNullException(nameof(train));
        }

        TrainId = train.TrainId;
        OwnerUsername = train.Owner.Username;
        IsMexicanTrain = train.IsMexicanTrain;
        IsOpen = train.IsOpen;
        IsLocalPlayerTrain = isLocalPlayerTrain;
        HiddenTileCount = Math.Max(NoHiddenTiles, train.Tiles.Count - MaxVisibleTiles);
        VisibleTiles = SelectVisibleTiles(train.Tiles, HiddenTileCount);
        CanReceiveTile = canReceiveTile;
        DropTileCommand = dropTileCommand ?? throw new ArgumentNullException(nameof(dropTileCommand));
    }

    public int TrainId { get; }

    public string OwnerUsername { get; }

    public bool IsMexicanTrain { get; }

    public bool IsOpen { get; }

    public bool IsLocalPlayerTrain { get; }

    public bool CanReceiveTile { get; }

    public int HiddenTileCount { get; }

    public bool HasHiddenTiles => HiddenTileCount > NoHiddenTiles;

    public IReadOnlyList<TileSummary> VisibleTiles { get; }

    public ICommand DropTileCommand { get; }

    private static List<TileSummary> SelectVisibleTiles(IReadOnlyList<TileSummary> tiles, int firstVisibleIndex)
    {
        List<TileSummary> visibleTiles = new List<TileSummary>();

        for (int tileIndex = firstVisibleIndex; tileIndex < tiles.Count; tileIndex++)
        {
            visibleTiles.Add(tiles[tileIndex]);
        }

        return visibleTiles;
    }
}
