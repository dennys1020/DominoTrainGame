using System.Collections.Generic;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal static class FriendStatusCodes
{
    private static readonly IReadOnlyDictionary<PlayerStatusCode, FriendStatus> _friendStatusesByCode;

    static FriendStatusCodes()
    {
        _friendStatusesByCode = new Dictionary<PlayerStatusCode, FriendStatus>
        {
            [PlayerStatusCode.Offline] = FriendStatus.Offline,
            [PlayerStatusCode.Online] = FriendStatus.Online,
            [PlayerStatusCode.InLobby] = FriendStatus.InLobby,
            [PlayerStatusCode.InMatch] = FriendStatus.InMatch
        };
    }

    public static FriendStatus ToFriendStatus(PlayerStatusCode statusCode)
    {
        bool isKnownStatus = _friendStatusesByCode.TryGetValue(statusCode, out FriendStatus knownStatus);
        FriendStatus status = isKnownStatus ? knownStatus : FriendStatus.Offline;
        return status;
    }
}
