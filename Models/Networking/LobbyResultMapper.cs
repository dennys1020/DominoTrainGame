using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal static class LobbyResultMapper
{
    public static LobbyResult MapLobbyResponse(LobbyResponse response)
    {
        LobbyResult result = LobbyResult.CreateFailure(LobbyResultStatus.ServerError);

        if (response != null)
        {
            result = new LobbyResult(
                LobbyResultStatus.Success,
                response.Code,
                response.MaximumPlayers,
                MapLobbyRoster(response));
        }

        return result;
    }

    public static LobbyPlayerResult MapLobbyPlayer(LobbyPlayer player)
    {
        PlayerSummary member = new PlayerSummary(player.PlayerId, player.Username);
        LobbyPlayerResult lobbyPlayer = new LobbyPlayerResult(member, player.IsReady);
        return lobbyPlayer;
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
}
