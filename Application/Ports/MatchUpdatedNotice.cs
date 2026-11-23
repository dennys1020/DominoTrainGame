using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class MatchUpdatedNotice
{
    public MatchUpdatedNotice(
        IReadOnlyList<int> recipientPlayerIds,
        int currentPlayerId,
        int boneyardCount,
        Tile engineTile,
        IReadOnlyDictionary<int, int> tileCountsByPlayerId,
        IReadOnlyDictionary<int, int> scoresByPlayerId,
        IReadOnlyList<TrainSnapshot> trains)
    {
        RecipientPlayerIds = new List<int>(
            recipientPlayerIds ?? throw new ArgumentNullException(nameof(recipientPlayerIds)));
        CurrentPlayerId = currentPlayerId;
        BoneyardCount = boneyardCount;
        EngineTile = engineTile ?? throw new ArgumentNullException(nameof(engineTile));
        TileCountsByPlayerId = tileCountsByPlayerId ?? throw new ArgumentNullException(nameof(tileCountsByPlayerId));
        ScoresByPlayerId = scoresByPlayerId ?? throw new ArgumentNullException(nameof(scoresByPlayerId));
        Trains = trains ?? throw new ArgumentNullException(nameof(trains));
    }

    public IReadOnlyList<int> RecipientPlayerIds { get; }

    public int CurrentPlayerId { get; }

    public int BoneyardCount { get; }

    public Tile EngineTile { get; }

    public IReadOnlyDictionary<int, int> TileCountsByPlayerId { get; }

    public IReadOnlyDictionary<int, int> ScoresByPlayerId { get; }

    public IReadOnlyList<TrainSnapshot> Trains { get; }
}
