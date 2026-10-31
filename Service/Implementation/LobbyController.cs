using System;
using System.Collections.Generic;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Connection;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Service.DTO;
using DominoTrainGame.Server.Service.Networking;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed class LobbyController
{
    private static readonly ITraceLog _logger;

    private readonly ILobbyUseCases _lobbyUseCases;
    private readonly SessionManager _sessionManager;

    static LobbyController()
    {
        _logger = TraceLogManager.GetLogger(typeof(LobbyController));
    }

    public LobbyController(ILobbyUseCases lobbyUseCases, SessionManager sessionManager)
    {
        _lobbyUseCases = lobbyUseCases ?? throw new ArgumentNullException(nameof(lobbyUseCases));
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

    private static LobbyResponse ToLobbyResponse(LobbyState lobby)
    {
        List<LobbyPlayer> players = new List<LobbyPlayer>();

        foreach (LobbyPlayerState player in lobby.Roster.Players)
        {
            LobbyPlayer lobbyPlayer = new LobbyPlayer
            {
                PlayerId = player.PlayerId,
                Username = player.Username,
                IsHost = player.IsHost
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
}
