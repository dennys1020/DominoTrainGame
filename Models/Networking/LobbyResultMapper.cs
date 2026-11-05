using System;
using System.Collections.Generic;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal static class LobbyResultMapper
{
    private const int NoHostPlayerId = 0;

    public static LobbyResult CreateEmptyLobbyResult(LobbyResultStatus status)
    {
        LobbyResult result = new LobbyResult(status, LobbySettings.Empty, LobbyRoster.Empty);
        return result;
    }

    public static LobbyResult MapLobbyResponse(LobbyResponse response)
    {
        LobbyResult result = CreateEmptyLobbyResult(LobbyResultStatus.ServerError);

        if (response != null)
        {
            LobbySettings settings = new LobbySettings(response.Code, response.MaximumPlayers);
            result = new LobbyResult(LobbyResultStatus.Success, settings, MapLobbyRoster(response));
        }

        return result;
    }

    private static LobbyRoster MapLobbyRoster(LobbyResponse response)
    {
        List<LobbyPlayerResult> players = new List<LobbyPlayerResult>();
        int hostPlayerId = NoHostPlayerId;

        foreach (LobbyPlayer player in response.Players ?? Array.Empty<LobbyPlayer>())
        {
            if (player != null)
            {
                players.Add(MapLobbyPlayer(player));
                hostPlayerId = player.IsHost ? player.PlayerId : hostPlayerId;
            }
        }

        LobbyRoster roster = new LobbyRoster(response.Version, hostPlayerId, players);
        return roster;
    }

    private static LobbyPlayerResult MapLobbyPlayer(LobbyPlayer player)
    {
        LobbyMember member = new LobbyMember(player.PlayerId, player.Username);
        LobbyPlayerResult lobbyPlayer = new LobbyPlayerResult(member, player.IsHost, player.IsReady);
        return lobbyPlayer;
    }
}
