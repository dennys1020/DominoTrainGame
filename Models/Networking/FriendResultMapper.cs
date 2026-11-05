using System.Collections.Generic;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal static class FriendResultMapper
{
    private static readonly IReadOnlyDictionary<GameServiceFaultCode, FriendResultStatus> _statusesByFaultCode;

    static FriendResultMapper()
    {
        _statusesByFaultCode = new Dictionary<GameServiceFaultCode, FriendResultStatus>
        {
            [GameServiceFaultCode.InvalidInput] = FriendResultStatus.InvalidInput,
            [GameServiceFaultCode.DatabaseUnavailable] = FriendResultStatus.DatabaseUnavailable,
            [GameServiceFaultCode.SessionNotFound] = FriendResultStatus.SessionNotFound,
            [GameServiceFaultCode.InvalidSessionToken] = FriendResultStatus.SessionNotFound,
            [GameServiceFaultCode.PlayerNotFound] = FriendResultStatus.PlayerNotFound,
            [GameServiceFaultCode.CannotTargetSelf] = FriendResultStatus.CannotTargetSelf,
            [GameServiceFaultCode.AlreadyFriends] = FriendResultStatus.AlreadyFriends,
            [GameServiceFaultCode.FriendRequestAlreadyExists] = FriendResultStatus.RequestAlreadyExists,
            [GameServiceFaultCode.PlayerBlocked] = FriendResultStatus.PlayerBlocked,
            [GameServiceFaultCode.FriendRequestNotFound] = FriendResultStatus.RequestNotFound,
            [GameServiceFaultCode.NotFriends] = FriendResultStatus.NotFriends
        };
    }

    public static FriendResult ToFriendResult(FriendResponse response)
    {
        FriendStatus status = FriendStatusCodes.ToFriendStatus(response.Status);
        FriendResult friend = new FriendResult(response.PlayerId, response.Username, status);
        return friend;
    }

    public static FriendRequestResult ToFriendRequestResult(PendingFriendRequestResponse response)
    {
        FriendRequestResult request = new FriendRequestResult(
            response.RequestId,
            response.RequesterUsername,
            response.RequestedAt);
        return request;
    }

    public static FriendListResult CreateFailedFriendList(FriendResultStatus status)
    {
        FriendListResult result = new FriendListResult(status, new List<FriendResult>());
        return result;
    }

    public static FriendRequestListResult CreateFailedRequestList(FriendResultStatus status)
    {
        FriendRequestListResult result = new FriendRequestListResult(status, new List<FriendRequestResult>());
        return result;
    }

    public static PlayerSearchResult CreateFailedPlayerSearch(FriendResultStatus status)
    {
        PlayerSearchResult result = new PlayerSearchResult(status, new List<FoundPlayerResult>());
        return result;
    }

    public static FriendResultStatus MapFault(GameServiceFaultCode faultCode)
    {
        bool isKnownFault = _statusesByFaultCode.TryGetValue(faultCode, out FriendResultStatus knownStatus);
        FriendResultStatus status = isKnownFault ? knownStatus : FriendResultStatus.ServerError;
        return status;
    }
}
