using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
    private readonly IFriendUseCases _friendUseCases;
    private readonly PlayerSessionManager<ISocialClientCallback> _socialSessionManager;
    private readonly ServiceFaultTranslator _faultTranslator;

    public FriendService(
        IFriendUseCases friendUseCases,
        PlayerSessionManager<ISocialClientCallback> socialSessionManager,
        ServiceFaultTranslator faultTranslator)
    {
        _friendUseCases = friendUseCases ?? throw new ArgumentNullException(nameof(friendUseCases));
        _socialSessionManager = socialSessionManager ?? throw new ArgumentNullException(nameof(socialSessionManager));
        _faultTranslator = faultTranslator ?? throw new ArgumentNullException(nameof(faultTranslator));
    }

    public void Connect(ConnectRequest request)
    {
        string sessionToken = request?.SessionToken ?? string.Empty;
        _socialSessionManager.Connect(sessionToken);
    }

    public List<FriendResponse> GetFriends()
    {
        int playerId = _socialSessionManager.ResolveCaller().PlayerId;
        IReadOnlyList<FriendPresence> friends;

        try
        {
            friends = _friendUseCases.GetFriends(playerId);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        List<FriendResponse> response = friends.Select(ToFriendResponse).ToList();

        return response;
    }

    public List<PendingFriendRequestResponse> GetPendingRequests()
    {
        int playerId = _socialSessionManager.ResolveCaller().PlayerId;
        IReadOnlyList<FriendRequestSummary> pendingRequests;

        try
        {
            pendingRequests = _friendUseCases.GetPendingRequests(playerId);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        List<PendingFriendRequestResponse> response = pendingRequests.Select(ToPendingRequestResponse).ToList();

        return response;
    }

    public List<PlayerSearchResponse> SearchPlayers(PlayerSearchRequest request)
    {
        string usernameFragment = request?.UsernameFragment ?? string.Empty;
        int playerId = _socialSessionManager.ResolveCaller().PlayerId;
        IReadOnlyList<PlayerIdentity> players;

        try
        {
            players = _friendUseCases.SearchPlayers(playerId, usernameFragment);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        List<PlayerSearchResponse> response = players.Select(ToPlayerSearchResponse).ToList();

        return response;
    }

    public void SendFriendRequest(FriendUsernameRequest request)
    {
        string addresseeUsername = request?.Username ?? string.Empty;
        int playerId = _socialSessionManager.ResolveCaller().PlayerId;
        SendFriendRequestStatus status;

        try
        {
            status = _friendUseCases.SendFriendRequest(playerId, addresseeUsername);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        GameFaultFactory.ThrowIfRejected(status, SendFriendRequestStatus.Success, MapSendFaultCode);
    }

    public void AcceptFriendRequest(FriendshipDecisionRequest request)
    {
        int requestId = request?.RequestId ?? default;
        int playerId = _socialSessionManager.ResolveCaller().PlayerId;
        bool isAccepted;

        try
        {
            isAccepted = _friendUseCases.AcceptFriendRequest(playerId, requestId);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        GameFaultFactory.ThrowIfRejected(isAccepted, GameServiceFaultCode.FriendRequestNotFound);
    }

    public void RejectFriendRequest(FriendshipDecisionRequest request)
    {
        int requestId = request?.RequestId ?? default;
        int playerId = _socialSessionManager.ResolveCaller().PlayerId;
        bool isRejected;

        try
        {
            isRejected = _friendUseCases.RejectFriendRequest(playerId, requestId);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        GameFaultFactory.ThrowIfRejected(isRejected, GameServiceFaultCode.FriendRequestNotFound);
    }

    public void RemoveFriend(FriendUsernameRequest request)
    {
        string friendUsername = request?.Username ?? string.Empty;
        int playerId = _socialSessionManager.ResolveCaller().PlayerId;
        RemoveFriendStatus status;

        try
        {
            status = _friendUseCases.RemoveFriend(playerId, friendUsername);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        GameFaultFactory.ThrowIfRejected(status, RemoveFriendStatus.Success, MapRemoveFaultCode);
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

    private static PlayerSearchResponse ToPlayerSearchResponse(PlayerIdentity player)
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
}
