using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Service.Implementation;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class FriendService : IFriendService
{
    private static readonly ITraceLog _logger;

    private readonly IFriendUseCases _friendUseCases;
    private readonly PlayerSessionManager<ISocialClientCallback> _socialSessionManager;

    static FriendService()
    {
        _logger = TraceLogManager.GetLogger(typeof(FriendService));
    }

    public FriendService(
        IFriendUseCases friendUseCases,
        PlayerSessionManager<ISocialClientCallback> socialSessionManager)
    {
        _friendUseCases = friendUseCases ?? throw new ArgumentNullException(nameof(friendUseCases));
        _socialSessionManager = socialSessionManager ?? throw new ArgumentNullException(nameof(socialSessionManager));
    }

    public void ConnectFriends(ConnectRequest request)
    {
        string sessionToken = request?.SessionToken ?? string.Empty;
        _socialSessionManager.Connect(sessionToken);
    }

    public List<FriendResponse> GetFriends()
    {
        IReadOnlyList<FriendPresence> friends = ExecuteForCaller(playerId => _friendUseCases.GetFriends(playerId));
        List<FriendResponse> response = friends.Select(ToFriendResponse).ToList();

        return response;
    }

    public List<PendingFriendRequestResponse> GetPendingRequests()
    {
        IReadOnlyList<FriendRequestSummary> pendingRequests = ExecuteForCaller(
            playerId => _friendUseCases.GetPendingRequests(playerId));
        List<PendingFriendRequestResponse> response = pendingRequests.Select(ToPendingRequestResponse).ToList();

        return response;
    }

    public List<PlayerSearchResponse> SearchPlayers(PlayerSearchRequest request)
    {
        string usernameFragment = request?.UsernameFragment ?? string.Empty;
        IReadOnlyList<FriendSummary> players = ExecuteForCaller(
            playerId => _friendUseCases.SearchPlayers(playerId, usernameFragment));
        List<PlayerSearchResponse> response = players.Select(ToPlayerSearchResponse).ToList();

        return response;
    }

    public void SendFriendRequest(FriendUsernameRequest request)
    {
        string addresseeUsername = request?.Username ?? string.Empty;
        SendFriendRequestStatus status = ExecuteForCaller(
            playerId => _friendUseCases.SendFriendRequest(playerId, addresseeUsername));

        if (status != SendFriendRequestStatus.Success)
        {
            throw GameFaultFactory.Create(MapSendFaultCode(status));
        }
    }

    public void AcceptFriendRequest(FriendshipDecisionRequest request)
    {
        int requestId = request?.RequestId ?? default;
        bool isAccepted = ExecuteForCaller(playerId => _friendUseCases.AcceptFriendRequest(playerId, requestId));

        if (!isAccepted)
        {
            throw GameFaultFactory.Create(GameServiceFaultCode.FriendRequestNotFound);
        }
    }

    public void RejectFriendRequest(FriendshipDecisionRequest request)
    {
        int requestId = request?.RequestId ?? default;
        bool isRejected = ExecuteForCaller(playerId => _friendUseCases.RejectFriendRequest(playerId, requestId));

        if (!isRejected)
        {
            throw GameFaultFactory.Create(GameServiceFaultCode.FriendRequestNotFound);
        }
    }

    public void RemoveFriend(FriendUsernameRequest request)
    {
        string friendUsername = request?.Username ?? string.Empty;
        RemoveFriendStatus status = ExecuteForCaller(
            playerId => _friendUseCases.RemoveFriend(playerId, friendUsername));

        if (status != RemoveFriendStatus.Success)
        {
            throw GameFaultFactory.Create(MapRemoveFaultCode(status));
        }
    }

    private static FriendResponse ToFriendResponse(FriendPresence friend)
    {
        FriendResponse response = new FriendResponse
        {
            PlayerId = friend.PlayerId,
            Username = friend.Username,
            Status = PlayerStatusCodes.FromStatus(friend.Status)
        };

        return response;
    }

    private static PendingFriendRequestResponse ToPendingRequestResponse(FriendRequestSummary pendingRequest)
    {
        PendingFriendRequestResponse response = new PendingFriendRequestResponse
        {
            RequestId = pendingRequest.RequestId,
            RequesterUsername = pendingRequest.RequesterUsername,
            RequestedAt = pendingRequest.RequestedAt
        };

        return response;
    }

    private static PlayerSearchResponse ToPlayerSearchResponse(FriendSummary player)
    {
        PlayerSearchResponse response = new PlayerSearchResponse
        {
            PlayerId = player.PlayerId,
            Username = player.Username
        };

        return response;
    }

    private static GameServiceFaultCode MapSendFaultCode(SendFriendRequestStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case SendFriendRequestStatus.PlayerNotFound:
                faultCode = GameServiceFaultCode.PlayerNotFound;
                break;
            case SendFriendRequestStatus.CannotTargetSelf:
                faultCode = GameServiceFaultCode.CannotTargetSelf;
                break;
            case SendFriendRequestStatus.AlreadyFriends:
                faultCode = GameServiceFaultCode.AlreadyFriends;
                break;
            case SendFriendRequestStatus.RequestAlreadyExists:
                faultCode = GameServiceFaultCode.FriendRequestAlreadyExists;
                break;
            case SendFriendRequestStatus.PlayerBlocked:
                faultCode = GameServiceFaultCode.PlayerBlocked;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidInput;
                break;
        }

        return faultCode;
    }

    private static GameServiceFaultCode MapRemoveFaultCode(RemoveFriendStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case RemoveFriendStatus.PlayerNotFound:
                faultCode = GameServiceFaultCode.PlayerNotFound;
                break;
            case RemoveFriendStatus.NotFriends:
                faultCode = GameServiceFaultCode.NotFriends;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidInput;
                break;
        }

        return faultCode;
    }

    private TResult ExecuteForCaller<TResult>(Func<int, TResult> operation)
    {
        int playerId = _socialSessionManager.ResolveCaller().PlayerId;
        TResult result;

        try
        {
            result = operation(playerId);
        }
        catch (ArgumentException exception)
        {
            _logger.Warn("A friends request was rejected because its input is invalid.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("A friends request failed because the database is unavailable.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.DatabaseUnavailable);
        }

        return result;
    }
}
