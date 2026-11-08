using System;
using System.Collections.Generic;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Service.Implementation;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class LobbyService : ILobbyService
{
    private static readonly ITraceLog _logger;

    private readonly ILobbyUseCases _lobbyUseCases;
    private readonly IInvitationUseCases _invitationUseCases;
    private readonly PlayerSessionManager<IGameClientCallback> _lobbySessionManager;

    static LobbyService()
    {
        _logger = TraceLogManager.GetLogger(typeof(LobbyService));
    }

    public LobbyService(
        ILobbyUseCases lobbyUseCases,
        IInvitationUseCases invitationUseCases,
        PlayerSessionManager<IGameClientCallback> lobbySessionManager)
    {
        _lobbyUseCases = lobbyUseCases ?? throw new ArgumentNullException(nameof(lobbyUseCases));
        _invitationUseCases = invitationUseCases ?? throw new ArgumentNullException(nameof(invitationUseCases));
        _lobbySessionManager = lobbySessionManager ?? throw new ArgumentNullException(nameof(lobbySessionManager));
    }

    public void Connect(ConnectRequest request)
    {
        string sessionToken = request?.SessionToken ?? string.Empty;
        _lobbySessionManager.Connect(sessionToken);
    }

    public LobbyResponse CreateLobby()
    {
        PlayerIdentity caller = _lobbySessionManager.ResolveCaller();
        LobbyState lobby = LobbyState.Empty;
        CreateLobbyStatus status = ExecuteLobbyRequest(
            () => _lobbyUseCases.CreateLobby(caller, out lobby),
            GameServiceFaultCode.InvalidLobbyRequest);

        if (status != CreateLobbyStatus.Success)
        {
            throw GameFaultFactory.Create(MapCreateFaultCode(status));
        }

        LobbyResponse response = ToLobbyResponse(lobby);
        return response;
    }

    public LobbyResponse JoinLobby(JoinLobbyRequest request)
    {
        PlayerIdentity caller = _lobbySessionManager.ResolveCaller();
        LobbyState lobby = LobbyState.Empty;
        JoinLobbyStatus status = ExecuteLobbyRequest(
            () => _lobbyUseCases.JoinLobby(caller, request?.Code, out lobby),
            GameServiceFaultCode.InvalidLobbyRequest);

        if (status != JoinLobbyStatus.Success)
        {
            throw GameFaultFactory.Create(MapJoinFaultCode(status));
        }

        LobbyResponse response = ToLobbyResponse(lobby);
        return response;
    }

    public void LeaveLobby()
    {
        PlayerIdentity caller = _lobbySessionManager.ResolveCaller();
        _lobbyUseCases.LeaveLobby(caller.PlayerId);
    }

    public void SetReady(SetReadyRequest request)
    {
        PlayerIdentity caller = _lobbySessionManager.ResolveCaller();

        if (request == null)
        {
            throw CreateMissingRequestFault();
        }

        bool wasUpdated = _lobbyUseCases.TrySetReady(caller.PlayerId, request.IsReady);

        if (!wasUpdated)
        {
            throw GameFaultFactory.Create(GameServiceFaultCode.LobbyNotFound);
        }
    }

    public void SendLobbyMessage(SendLobbyMessageRequest request)
    {
        PlayerIdentity caller = _lobbySessionManager.ResolveCaller();

        if (request == null)
        {
            throw CreateMissingRequestFault();
        }

        SendMessageStatus status = ExecuteLobbyRequest(
            () => _lobbyUseCases.SendMessage(caller.PlayerId, request.Text),
            GameServiceFaultCode.InvalidInput);

        if (status != SendMessageStatus.Sent)
        {
            throw GameFaultFactory.Create(MapSendMessageFaultCode(status));
        }
    }

    public void StartMatch()
    {
        PlayerIdentity caller = _lobbySessionManager.ResolveCaller();
        StartMatchStatus status = _lobbyUseCases.StartMatch(caller.PlayerId);

        if (status != StartMatchStatus.Success)
        {
            throw GameFaultFactory.Create(MapStartFaultCode(status));
        }
    }

    public void InviteFriend(FriendUsernameRequest request)
    {
        PlayerIdentity caller = _lobbySessionManager.ResolveCaller();

        if (request == null)
        {
            throw CreateMissingRequestFault();
        }

        InviteFriendStatus status = ExecuteLobbyRequest(
            () => _invitationUseCases.InviteFriend(caller, request.Username),
            GameServiceFaultCode.InvalidInput);

        if (status != InviteFriendStatus.Success)
        {
            throw GameFaultFactory.Create(MapInviteFaultCode(status));
        }
    }

    private static TResult ExecuteLobbyRequest<TResult>(
        Func<TResult> request,
        GameServiceFaultCode invalidInputFaultCode)
    {
        TResult result;

        try
        {
            result = request();
        }
        catch (ArgumentException exception)
        {
            _logger.Warn("A lobby request was rejected because its input is invalid.", exception);
            throw GameFaultFactory.Create(invalidInputFaultCode);
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("A lobby request failed because the database is unavailable.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.DatabaseUnavailable);
        }

        return result;
    }

    private static FaultException<GameServiceFault> CreateMissingRequestFault()
    {
        _logger.Warn("A lobby request was rejected because it has no content.");
        FaultException<GameServiceFault> fault = GameFaultFactory.Create(GameServiceFaultCode.InvalidLobbyRequest);
        return fault;
    }

    private static LobbyResponse ToLobbyResponse(LobbyState lobby)
    {
        List<LobbyPlayer> players = new List<LobbyPlayer>();

        foreach (LobbyPlayerState player in lobby.Roster.Players)
        {
            LobbyPlayer lobbyPlayer = new LobbyPlayer
            {
                PlayerId = player.PlayerId,
                Username = player.Username,
                IsReady = player.IsReady
            };

            players.Add(lobbyPlayer);
        }

        LobbyResponse response = new LobbyResponse
        {
            Code = lobby.Code,
            MaximumPlayers = lobby.MaximumPlayers,
            Version = lobby.Roster.Version,
            HostPlayerId = lobby.Roster.HostPlayerId,
            Players = players
        };

        return response;
    }

    private static GameServiceFaultCode MapCreateFaultCode(CreateLobbyStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case CreateLobbyStatus.AlreadyInLobby:
                faultCode = GameServiceFaultCode.LobbyConflict;
                break;
            case CreateLobbyStatus.PlayerBanned:
                faultCode = GameServiceFaultCode.PlayerBanned;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidLobbyRequest;
                break;
        }

        return faultCode;
    }

    private static GameServiceFaultCode MapJoinFaultCode(JoinLobbyStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case JoinLobbyStatus.LobbyNotFound:
                faultCode = GameServiceFaultCode.LobbyNotFound;
                break;
            case JoinLobbyStatus.LobbyFull:
                faultCode = GameServiceFaultCode.LobbyFull;
                break;
            case JoinLobbyStatus.AlreadyInLobby:
                faultCode = GameServiceFaultCode.LobbyConflict;
                break;
            case JoinLobbyStatus.PlayerBanned:
                faultCode = GameServiceFaultCode.PlayerBanned;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidLobbyRequest;
                break;
        }

        return faultCode;
    }

    private static GameServiceFaultCode MapSendMessageFaultCode(SendMessageStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case SendMessageStatus.Warned:
                faultCode = GameServiceFaultCode.ChatWarning;
                break;
            case SendMessageStatus.ChatBlocked:
                faultCode = GameServiceFaultCode.ChatBlocked;
                break;
            default:
                faultCode = GameServiceFaultCode.LobbyNotFound;
                break;
        }

        return faultCode;
    }

    private static GameServiceFaultCode MapInviteFaultCode(InviteFriendStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case InviteFriendStatus.LobbyNotFound:
                faultCode = GameServiceFaultCode.LobbyNotFound;
                break;
            case InviteFriendStatus.LobbyFull:
                faultCode = GameServiceFaultCode.LobbyFull;
                break;
            case InviteFriendStatus.PlayerNotFound:
                faultCode = GameServiceFaultCode.PlayerNotFound;
                break;
            case InviteFriendStatus.NotFriends:
                faultCode = GameServiceFaultCode.NotFriends;
                break;
            case InviteFriendStatus.PlayerOffline:
                faultCode = GameServiceFaultCode.PlayerOffline;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidLobbyRequest;
                break;
        }

        return faultCode;
    }

    private static GameServiceFaultCode MapStartFaultCode(StartMatchStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case StartMatchStatus.LobbyNotFound:
                faultCode = GameServiceFaultCode.LobbyNotFound;
                break;
            case StartMatchStatus.NotLobbyHost:
                faultCode = GameServiceFaultCode.NotLobbyHost;
                break;
            case StartMatchStatus.PlayersNotReady:
                faultCode = GameServiceFaultCode.PlayersNotReady;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidLobbyRequest;
                break;
        }

        return faultCode;
    }
}
