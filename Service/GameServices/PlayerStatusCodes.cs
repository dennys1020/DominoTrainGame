using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Service.DataContracts;

namespace DominoTrainGame.Server.Service.Implementation;

internal static class PlayerStatusCodes
{
    private static readonly IReadOnlyDictionary<PlayerStatus, PlayerStatusCode> _statusCodesByStatus;

    static PlayerStatusCodes()
    {
        _statusCodesByStatus = new Dictionary<PlayerStatus, PlayerStatusCode>
        {
            [PlayerStatus.Offline] = PlayerStatusCode.Offline,
            [PlayerStatus.Online] = PlayerStatusCode.Online,
            [PlayerStatus.InLobby] = PlayerStatusCode.InLobby,
            [PlayerStatus.InMatch] = PlayerStatusCode.InMatch
        };
    }

    public static PlayerStatusCode FromStatus(PlayerStatus status)
    {
        PlayerStatusCode statusCode = _statusCodesByStatus.ContainsKey(status)
            ? _statusCodesByStatus[status]
            : PlayerStatusCode.Offline;
        return statusCode;
    }
}
