using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Service.Callbacks;

public sealed class CallbackGameNotifier : IGameNotifier
{
    private readonly PlayerSessionRegistry<IGameClientCallback> _lobbySessions;

    public CallbackGameNotifier(PlayerSessionRegistry<IGameClientCallback> lobbySessions)
    {
        _lobbySessions = lobbySessions ?? throw new ArgumentNullException(nameof(lobbySessions));
    }

    public void NotifyPlayerJoined(PlayerJoinedNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        PlayerJoinedMessage message = new PlayerJoinedMessage
        {
            Player = new LobbyPlayer
            {
                PlayerId = notice.Member.PlayerId,
                Username = notice.Member.Username
            },
            Version = notice.Version
        };

        Broadcast(notice.RecipientPlayerIds, callback => callback.ReceivePlayerJoined(message));
    }

    public void NotifyPlayerLeft(PlayerLeftNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        PlayerLeftMessage message = new PlayerLeftMessage
        {
            PlayerId = notice.PlayerId,
            Version = notice.Version,
            HostPlayerId = notice.HostPlayerId
        };

        Broadcast(notice.RecipientPlayerIds, callback => callback.ReceivePlayerLeft(message));
    }

    public void NotifyPlayerReadyChanged(PlayerReadyChangedNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        PlayerReadyChangedMessage message = new PlayerReadyChangedMessage
        {
            Version = notice.Version,
            PlayerId = notice.Player.PlayerId,
            IsReady = notice.Player.IsReady
        };

        Broadcast(notice.RecipientPlayerIds, callback => callback.ReceivePlayerReadyChanged(message));
    }

    public void NotifyLobbyMessage(LobbyMessageNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        LobbyChatMessage message = new LobbyChatMessage
        {
            PlayerId = notice.Sender.PlayerId,
            Username = notice.Sender.Username,
            Text = notice.Text
        };

        Broadcast(notice.RecipientPlayerIds, callback => callback.ReceiveLobbyMessage(message));
    }

    public void NotifyMatchStarting(MatchStartingNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        MatchStartingMessage message = new MatchStartingMessage
        {
            Code = notice.Code
        };

        Broadcast(notice.RecipientPlayerIds, callback => callback.ReceiveMatchStarting(message));
    }

    public bool TryNotifyLobbyInvitation(LobbyInvitationNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        LobbyInvitationMessage message = new LobbyInvitationMessage
        {
            InviterUsername = notice.Inviter.Username,
            Code = notice.Code
        };

        bool isConnected = _lobbySessions.TrySend(
            notice.RecipientPlayerId,
            callback => callback.ReceiveLobbyInvitation(message));

        return isConnected;
    }

    public void NotifyMatchUpdated(MatchUpdatedNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        MatchSnapshotMessage message = new MatchSnapshotMessage
        {
            CurrentPlayerId = notice.CurrentPlayerId,
            BoneyardCount = notice.BoneyardCount,
            EngineTile = ToTileData(notice.EngineTile),
            Players = ToPlayerData(notice.TileCountsByPlayerId),
            Trains = ToTrainData(notice.Trains)
        };

        Broadcast(notice.RecipientPlayerIds,
            callback => callback.ReceiveMatchUpdate(message));
    }

    public void NotifyHandUpdated(HandUpdatedNotice notice)
    {
        if (notice == null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        HandMessage message = new HandMessage
        {
            Tiles = ToTileData(notice.Tiles)
        };

        _lobbySessions.TrySend(notice.RecipientPlayerId,
            callback => callback.ReceiveHand(message));
    }

    private static TileData ToTileData(Tile tile)
    {
        TileData tileData = new TileData
        {
            LeftPips = tile.LeftPips,
            RightPips = tile.RightPips
        };

        return tileData;
    }

    private static List<MatchPlayerData> ToPlayerData(IReadOnlyDictionary<int, int> tileCountsByPlayerId)
    {
        List<MatchPlayerData> players = new List<MatchPlayerData>();

        foreach (KeyValuePair<int, int> tileCount in tileCountsByPlayerId)
        {
            MatchPlayerData player = new MatchPlayerData
            {
                PlayerId = tileCount.Key,
                TileCount = tileCount.Value
            };

            players.Add(player);
        }

        return players;
    }

    private static List<TileData> ToTileData(IReadOnlyList<Tile> tiles)
    {
        List<TileData> tileData = new List<TileData>();

        foreach (Tile tile in tiles)
        {
            tileData.Add(ToTileData(tile));
        }

        return tileData;
    }

    private static List<TrainData> ToTrainData(IReadOnlyList<TrainSnapshot> trains)
    {
        List<TrainData> trainData = new List<TrainData>();

        foreach (TrainSnapshot train in trains)
        {
            TrainData data = new TrainData
            {
                OwnerId = train.OwnerId,
                IsOpen = train.IsOpen,
                Tiles = ToTileData(train.Tiles)
            };

            trainData.Add(data);
        }

        return trainData;
    }

    private void Broadcast(IReadOnlyList<int> recipientPlayerIds, Action<IGameClientCallback> send)
    {
        foreach (int recipientPlayerId in recipientPlayerIds)
        {
            _lobbySessions.TrySend(recipientPlayerId, send);
        }
    }
}
