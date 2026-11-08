using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal static class LobbyResultMapper
{
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
        List<LobbyPlayerResult> players = (response.Players ?? Array.Empty<LobbyPlayer>())
            .Where(player => player != null)
            .Select(MapLobbyPlayer)
            .ToList();
        LobbyRoster roster = new LobbyRoster(response.Version, response.HostPlayerId, players);

        return roster;
    }

    private static LobbyPlayerResult MapLobbyPlayer(LobbyPlayer player)
    {
        LobbyMember member = new LobbyMember(player.PlayerId, player.Username);
        LobbyPlayerResult lobbyPlayer = new LobbyPlayerResult(member, player.IsReady);
        return lobbyPlayer;
    }
}
