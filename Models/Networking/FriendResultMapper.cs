using System.Collections.Generic;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal static class FriendResultMapper
{
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
        PlayerSearchResult result = new PlayerSearchResult(status, new List<PlayerSummary>());
        return result;
    }

    public static FriendResultStatus MapFault(GameServiceFaultCode faultCode)
    {
        FriendResultStatus status;

        switch (faultCode)
        {
            case GameServiceFaultCode.InvalidInput:
                status = FriendResultStatus.InvalidInput;
                break;
            case GameServiceFaultCode.DatabaseUnavailable:
                status = FriendResultStatus.DatabaseUnavailable;
                break;
            case GameServiceFaultCode.SessionNotFound:
            case GameServiceFaultCode.InvalidSessionToken:
                status = FriendResultStatus.SessionNotFound;
                break;
            case GameServiceFaultCode.PlayerNotFound:
                status = FriendResultStatus.PlayerNotFound;
                break;
            default:
                status = MapFriendshipFault(faultCode);
                break;
        }

        return status;
    }

    private static FriendResultStatus MapFriendshipFault(GameServiceFaultCode faultCode)
    {
        FriendResultStatus status;

        switch (faultCode)
        {
            case GameServiceFaultCode.CannotTargetSelf:
                status = FriendResultStatus.CannotTargetSelf;
                break;
            case GameServiceFaultCode.AlreadyFriends:
                status = FriendResultStatus.AlreadyFriends;
                break;
            case GameServiceFaultCode.FriendRequestAlreadyExists:
                status = FriendResultStatus.RequestAlreadyExists;
                break;
            case GameServiceFaultCode.PlayerBlocked:
                status = FriendResultStatus.PlayerBlocked;
                break;
            case GameServiceFaultCode.FriendRequestNotFound:
                status = FriendResultStatus.RequestNotFound;
                break;
            case GameServiceFaultCode.NotFriends:
                status = FriendResultStatus.NotFriends;
                break;
            default:
                status = FriendResultStatus.ServerError;
                break;
        }

        return status;
    }
}
