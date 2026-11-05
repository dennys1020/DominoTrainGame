using System;
using System.Collections.Generic;
using DominoTrainGame.Client.Models.Contracts;

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

    public static LobbyResult CreateLobbyWithPlayer(LobbyResult lobby, PlayerJoinedEventArgs eventArgs)
    {
        List<LobbyPlayerResult> players = new List<LobbyPlayerResult>(lobby.Players);
        bool alreadyPresent = players.Exists(player => player.PlayerId == eventArgs.Player.PlayerId);

        if (!alreadyPresent)
        {
            players.Add(eventArgs.Player);
        }

        LobbyRoster roster = new LobbyRoster(eventArgs.Version, lobby.HostPlayerId, players);
        LobbyResult updatedLobby = lobby.WithRoster(roster);
        return updatedLobby;
    }

    public static LobbyResult CreateLobbyWithoutPlayer(LobbyResult lobby, PlayerLeftEventArgs eventArgs)
    {
        List<LobbyPlayerResult> players = new List<LobbyPlayerResult>();

        foreach (LobbyPlayerResult player in lobby.Players)
        {
            if (player.PlayerId != eventArgs.PlayerId)
            {
                bool isHost = player.PlayerId == eventArgs.HostPlayerId;
                players.Add(player.WithHostState(isHost));
            }
        }

        LobbyRoster roster = new LobbyRoster(eventArgs.Version, eventArgs.HostPlayerId, players);
        LobbyResult updatedLobby = lobby.WithRoster(roster);
        return updatedLobby;
    }

    public static LobbyResult CreateLobbyWithReadyState(LobbyResult lobby, PlayerReadyChangedEventArgs eventArgs)
    {
        List<LobbyPlayerResult> players = new List<LobbyPlayerResult>();

        foreach (LobbyPlayerResult player in lobby.Players)
        {
            LobbyPlayerResult updatedPlayer = player.PlayerId == eventArgs.PlayerId
                ? player.WithReadyState(eventArgs.IsReady)
                : player;

            players.Add(updatedPlayer);
        }

        LobbyRoster roster = new LobbyRoster(eventArgs.Version, lobby.HostPlayerId, players);
        LobbyResult updatedLobby = lobby.WithRoster(roster);
        return updatedLobby;
    }
}
