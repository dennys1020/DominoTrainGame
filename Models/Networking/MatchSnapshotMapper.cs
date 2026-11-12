using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal static class MatchSnapshotMapper
{
    private const int MexicanTrainOwnerId = 0;
    private const int PipValueCount = 13;

    public static MatchSnapshot ToMatchSnapshot(MatchSnapshotMessage message, LobbyResult lobby)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (lobby == null)
        {
            throw new ArgumentNullException(nameof(lobby));
        }

        MatchTurnSummary turn = new
        MatchTurnSummary(MatchSnapshot.Empty.Version,
            message.CurrentPlayerId);
        MatchTableSummary table = new
            MatchTableSummary(GetEnginePips(message.EngineTile),
            message.BoneyardCount,
            MapTrains(message.Trains, lobby));
        MatchSnapshot snapshot = new
            MatchSnapshot(turn, table,
            MapPlayers(message.Players, lobby));

        return snapshot;
    }

    public static PlayerHand ToPlayerHand(HandMessage message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        PlayerHand hand = new PlayerHand(PlayerHand.Empty.Version, MapTiles(message.Tiles));

        return hand;
    }

    private static int GetEnginePips(TileData engineTile)
    {
        int enginePips = engineTile?.LeftPips ?? MatchSnapshot.Empty.EnginePips;

        return enginePips;
    }

    private static List<TrainSummary> MapTrains(IEnumerable<TrainData> trains, LobbyResult lobby)
    {
        List<TrainSummary> trainSummaries = (trains ?? Enumerable.Empty<TrainData>())
            .Where(train => train != null)
            .Select(train => MapTrain(train, lobby))
            .ToList();

        return trainSummaries;
    }

    private static TrainSummary MapTrain(TrainData train, LobbyResult lobby)
    {
        PlayerSummary owner = new PlayerSummary(train.OwnerId, FindUsername(lobby, train.OwnerId));
        TrainSummary emptyTrain = new TrainSummary(train.OwnerId, owner, GetTrainStatus(train));
        TrainSummary trainSummary = emptyTrain.WithTiles(MapTiles(train.Tiles));

        return trainSummary;
    }

    private static TrainStatus GetTrainStatus(TrainData train)
    {
        TrainStatus status = TrainStatus.Closed;

        if (train.OwnerId == MexicanTrainOwnerId)
        {
            status = TrainStatus.Mexican;
        }
        else if (train.IsOpen)
        {
            status = TrainStatus.Open;
        }

        return status;
    }

    private static string FindUsername(LobbyResult lobby, int playerId)
    {
        string username = lobby.Players
            .Where(player => player.PlayerId == playerId)
            .Select(player => player.Username)
            .FirstOrDefault();

        return username ?? string.Empty;
    }

    private static List<TileSummary> MapTiles(IEnumerable<TileData> tiles)
    {
        List<TileSummary> tileSummaries =(tiles ?? Enumerable.Empty<TileData>())
            .Where(tile => tile != null)
            .Select(MapTile)
            .ToList();

        return tileSummaries;
    }

    private static TileSummary MapTile(TileData tile)
    {
        int lowPips = Math.Min(tile.LeftPips, tile.RightPips);
        int highPips = Math.Max(tile.LeftPips, tile.RightPips);
        int tileId = lowPips * PipValueCount + highPips;
        TileSummary tileSummary = new TileSummary(tileId, tile.LeftPips, tile.RightPips);

        return tileSummary;
    }

    private static List<MatchPlayerSummary> MapPlayers(IEnumerable<MatchPlayerData> players, LobbyResult lobby)
    {
        List<MatchPlayerSummary> playerSummaries = (players ?? Enumerable.Empty<MatchPlayerData>())
            .Where(player => player != null)
            .Select(player => MapPlayer(player, lobby))
            .ToList();

        return playerSummaries;
    }

    private static MatchPlayerSummary MapPlayer(MatchPlayerData player, LobbyResult lobby)
    {
        PlayerSummary playerSummary = new PlayerSummary(player.PlayerId, FindUsername(lobby, player.PlayerId));
        MatchPlayerSummary matchPlayerSummary = new MatchPlayerSummary(playerSummary, player.TileCount);

        return matchPlayerSummary;
    }
}
