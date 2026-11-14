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

    public static PendingFriendshipResult ToPendingFriendshipResult(PendingFriendshipResponse response)
    {
        PendingFriendshipResult request = new PendingFriendshipResult(
            response.RequestId,
            response.RequesterUsername,
            response.RequestedAt);
        return request;
    }

    public static FriendListResult CreateFailedFriendList(FriendActionResult status)
    {
        FriendListResult result = new FriendListResult(status, new List<FriendResult>());
        return result;
    }

    public static PendingFriendshipListResult CreateFailedRequestList(FriendActionResult status)
    {
        PendingFriendshipListResult result = new PendingFriendshipListResult(
            status,
            new List<PendingFriendshipResult>());
        return result;
    }

    public static PlayerSearchResult CreateFailedPlayerSearch(FriendActionResult status)
    {
        PlayerSearchResult result = new PlayerSearchResult(status, new List<PlayerSummary>());
        return result;
    }

    public static FriendActionResult MapFault(GameServiceFaultCode faultCode)
    {
        FriendActionResult status;

        switch (faultCode)
        {
            case GameServiceFaultCode.InvalidInput:
                status = FriendActionResult.InvalidInput;
                break;
            case GameServiceFaultCode.DatabaseUnavailable:
                status = FriendActionResult.DatabaseUnavailable;
                break;
            case GameServiceFaultCode.SessionNotFound:
            case GameServiceFaultCode.InvalidSessionToken:
                status = FriendActionResult.SessionNotFound;
                break;
            case GameServiceFaultCode.PlayerNotFound:
                status = FriendActionResult.PlayerNotFound;
                break;
            default:
                status = MapFriendshipFault(faultCode);
                break;
        }

        return status;
    }

    private static FriendActionResult MapFriendshipFault(GameServiceFaultCode faultCode)
    {
        FriendActionResult status;

        switch (faultCode)
        {
            case GameServiceFaultCode.CannotTargetSelf:
                status = FriendActionResult.CannotTargetSelf;
                break;
            case GameServiceFaultCode.AlreadyFriends:
                status = FriendActionResult.AlreadyFriends;
                break;
            case GameServiceFaultCode.FriendRequestAlreadyExists:
                status = FriendActionResult.RequestAlreadyExists;
                break;
            case GameServiceFaultCode.PlayerBlocked:
                status = FriendActionResult.PlayerBlocked;
                break;
            case GameServiceFaultCode.FriendRequestNotFound:
                status = FriendActionResult.RequestNotFound;
                break;
            case GameServiceFaultCode.NotFriends:
                status = FriendActionResult.NotFriends;
                break;
            default:
                status = FriendActionResult.ServerError;
                break;
        }

        return status;
    }
}
