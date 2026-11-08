using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal static class LobbyRosterUpdates
{
    public static LobbyResult ApplyIfNewer(LobbyResult lobby, long version, Func<LobbyResult, LobbyResult> update)
    {
        LobbyResult updatedLobby = lobby;
        bool isNewerVersion = version > lobby.Version;

        if (isNewerVersion)
        {
            updatedLobby = update(lobby);
        }

        return updatedLobby;
    }

    public static LobbyResult CreateLobbyWithPlayer(LobbyResult lobby, PlayerJoinedMessage message)
    {
        LobbyPlayerResult joinedPlayer = LobbyResultMapper.MapLobbyPlayer(message.Player);
        List<LobbyPlayerResult> players = new List<LobbyPlayerResult>(lobby.Players);
        bool isAlreadyPresent = players.Exists(player => player.PlayerId == joinedPlayer.PlayerId);

        if (!isAlreadyPresent)
        {
            players.Add(joinedPlayer);
        }

        LobbyRoster roster = new LobbyRoster(message.Version, lobby.HostPlayerId, players);
        LobbyResult updatedLobby = lobby.WithRoster(roster);
        return updatedLobby;
    }

    public static LobbyResult CreateLobbyWithoutPlayer(LobbyResult lobby, PlayerLeftMessage message)
    {
        List<LobbyPlayerResult> players = lobby.Players
            .Where(player => player.PlayerId != message.PlayerId)
            .ToList();
        LobbyRoster roster = new LobbyRoster(message.Version, message.HostPlayerId, players);
        LobbyResult updatedLobby = lobby.WithRoster(roster);

        return updatedLobby;
    }

    public static LobbyResult CreateLobbyWithReadyState(LobbyResult lobby, PlayerReadyChangedMessage message)
    {
        List<LobbyPlayerResult> players = new List<LobbyPlayerResult>();

        foreach (LobbyPlayerResult player in lobby.Players)
        {
            LobbyPlayerResult updatedPlayer = player.PlayerId == message.PlayerId
                ? player.WithReadyState(message.IsReady)
                : player;

            players.Add(updatedPlayer);
        }

        LobbyRoster roster = new LobbyRoster(message.Version, lobby.HostPlayerId, players);
        LobbyResult updatedLobby = lobby.WithRoster(roster);
        return updatedLobby;
    }
}
