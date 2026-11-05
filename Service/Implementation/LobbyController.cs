using System;
using System.Collections.Generic;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed class LobbyController
{
    private static readonly ITraceLog _logger;

    private readonly ILobbyUseCases _lobbyUseCases;
    private readonly IInvitationUseCases _invitationUseCases;
    private readonly SessionManager _sessionManager;

    static LobbyController()
    {
        _logger = TraceLogManager.GetLogger(typeof(LobbyController));
    }

    public LobbyController(
        ILobbyUseCases lobbyUseCases,
        IInvitationUseCases invitationUseCases,
        SessionManager sessionManager)
    {
        _lobbyUseCases = lobbyUseCases ?? throw new ArgumentNullException(nameof(lobbyUseCases));
        _invitationUseCases = invitationUseCases ?? throw new ArgumentNullException(nameof(invitationUseCases));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
    }

    public void Connect(ConnectRequest request)
    {
        string sessionToken = request?.SessionToken ?? string.Empty;
        _sessionManager.Connect(sessionToken);
    }

    public LobbyResponse CreateLobby()
    {
        AuthenticatedPlayer caller = _sessionManager.ResolveCaller();
        LobbyMember host = new LobbyMember(caller.PlayerId, caller.Username);
        bool wasCreated = _lobbyUseCases.TryCreateLobby(host, out LobbyState lobby);

        if (!wasCreated)
        {
            throw GameFaultFactory.Create(GameServiceFaultCode.LobbyConflict);
        }

        LobbyResponse response = ToLobbyResponse(lobby);
        return response;
    }

    public LobbyResponse JoinLobby(JoinLobbyRequest request)
    {
        AuthenticatedPlayer caller = _sessionManager.ResolveCaller();
        LobbyMember member = new LobbyMember(caller.PlayerId, caller.Username);
        LobbyResponse response;

        try
        {
            JoinLobbyStatus status = _lobbyUseCases.JoinLobby(member, request?.Code, out LobbyState lobby);

            if (status != JoinLobbyStatus.Success)
            {
                throw GameFaultFactory.Create(MapJoinFaultCode(status));
            }

            response = ToLobbyResponse(lobby);
        }
        catch (ArgumentException exception)
        {
            _logger.Warn("A join request was rejected because its input is invalid.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.InvalidLobbyRequest);
        }

        return response;
    }

    public void LeaveLobby()
    {
        AuthenticatedPlayer caller = _sessionManager.ResolveCaller();
        _lobbyUseCases.LeaveLobby(caller.PlayerId);
    }

    public void SetReady(SetReadyRequest request)
    {
        AuthenticatedPlayer caller = _sessionManager.ResolveCaller();

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
        AuthenticatedPlayer caller = _sessionManager.ResolveCaller();

        if (request == null)
        {
            throw CreateMissingRequestFault();
        }

        SendMessageStatus status;

        try
        {
            status = _lobbyUseCases.SendMessage(caller.PlayerId, request.Text);
        }
        catch (ArgumentException exception)
        {
            _logger.Warn("A lobby message was rejected because its text is invalid.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("A lobby message failed because the database is unavailable.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.DatabaseUnavailable);
        }

        if (status != SendMessageStatus.Sent)
        {
            throw GameFaultFactory.Create(MapSendMessageFaultCode(status));
        }
    }

    public void StartMatch()
    {
        AuthenticatedPlayer caller = _sessionManager.ResolveCaller();
        StartMatchStatus status = _lobbyUseCases.StartMatch(caller.PlayerId);

        if (status != StartMatchStatus.Success)
        {
            throw GameFaultFactory.Create(MapStartFaultCode(status));
        }
    }

    public void InviteFriend(FriendUsernameRequest request)
    {
        AuthenticatedPlayer caller = _sessionManager.ResolveCaller();

        if (request == null)
        {
            throw CreateMissingRequestFault();
        }

        LobbyMember inviter = new LobbyMember(caller.PlayerId, caller.Username);
        InviteFriendStatus status = SendInvitation(inviter, request.Username);

        if (status != InviteFriendStatus.Success)
        {
            throw GameFaultFactory.Create(MapInviteFaultCode(status));
        }
    }

    private InviteFriendStatus SendInvitation(LobbyMember inviter, string friendUsername)
    {
        InviteFriendStatus status;

        try
        {
            status = _invitationUseCases.InviteFriend(inviter, friendUsername);
        }
        catch (ArgumentException exception)
        {
            _logger.Warn("A lobby invitation was rejected because its username is invalid.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("A lobby invitation failed because the database is unavailable.", exception);
            throw GameFaultFactory.Create(GameServiceFaultCode.DatabaseUnavailable);
        }

        return status;
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
                IsHost = player.IsHost,
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
            default:
                faultCode = GameServiceFaultCode.InvalidLobbyRequest;
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
