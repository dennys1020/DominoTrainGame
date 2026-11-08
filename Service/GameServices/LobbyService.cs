using System;
using System.Collections.Generic;
using System.ServiceModel;
using DominoTrainGame.Server.Application.Domain;
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
    private readonly ILobbyUseCases _lobbyUseCases;
    private readonly IInvitationUseCases _invitationUseCases;
    private readonly PlayerSessionManager<IGameClientCallback> _lobbySessionManager;
    private readonly ServiceFaultTranslator _faultTranslator;

    public LobbyService(
        ILobbyUseCases lobbyUseCases,
        IInvitationUseCases invitationUseCases,
        PlayerSessionManager<IGameClientCallback> lobbySessionManager,
        ServiceFaultTranslator faultTranslator)
    {
        _lobbyUseCases = lobbyUseCases ?? throw new ArgumentNullException(nameof(lobbyUseCases));
        _invitationUseCases = invitationUseCases ?? throw new ArgumentNullException(nameof(invitationUseCases));
        _lobbySessionManager = lobbySessionManager ?? throw new ArgumentNullException(nameof(lobbySessionManager));
        _faultTranslator = faultTranslator ?? throw new ArgumentNullException(nameof(faultTranslator));
    }

    public void Connect(ConnectRequest request)
    {
        string sessionToken = request?.SessionToken ?? string.Empty;
        _lobbySessionManager.Connect(sessionToken);
    }

    public LobbyResponse CreateLobby()
    {
        LobbyState lobby = LobbyState.Empty;
        CreateLobbyStatus status = ExecuteForCaller(
            caller => _lobbyUseCases.CreateLobby(caller, out lobby),
            GameServiceFaultCode.InvalidLobbyRequest);

        GameFaultFactory.ThrowIfRejected(status, CreateLobbyStatus.Success, MapCreateFaultCode);
        LobbyResponse response = ToLobbyResponse(lobby);
        return response;
    }

    public LobbyResponse JoinLobby(JoinLobbyRequest request)
    {
        LobbyState lobby = LobbyState.Empty;
        JoinLobbyStatus status = ExecuteForCaller(
            caller => _lobbyUseCases.JoinLobby(caller, request?.Code, out lobby),
            GameServiceFaultCode.InvalidLobbyRequest);

        GameFaultFactory.ThrowIfRejected(status, JoinLobbyStatus.Success, MapJoinFaultCode);
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
        bool isReady = request?.IsReady ?? false;
        bool wasUpdated = ExecuteForCaller(
            caller => _lobbyUseCases.TrySetReady(caller.PlayerId, isReady),
            GameServiceFaultCode.InvalidInput);

        GameFaultFactory.ThrowIfRejected(wasUpdated, GameServiceFaultCode.LobbyNotFound);
    }

    public void SendLobbyMessage(SendLobbyMessageRequest request)
    {
        SendMessageStatus status = ExecuteForCaller(
            caller => _lobbyUseCases.SendMessage(caller.PlayerId, request?.Text),
            GameServiceFaultCode.InvalidInput);

        GameFaultFactory.ThrowIfRejected(status, SendMessageStatus.Success, MapSendMessageFaultCode);
    }

    public void StartMatch()
    {
        StartMatchStatus status = ExecuteForCaller(
            caller => _lobbyUseCases.StartMatch(caller.PlayerId),
            GameServiceFaultCode.InvalidLobbyRequest);

        GameFaultFactory.ThrowIfRejected(status, StartMatchStatus.Success, MapStartFaultCode);
    }

    public void InviteFriend(FriendUsernameRequest request)
    {
        InviteFriendStatus status = ExecuteForCaller(
            caller => _invitationUseCases.InviteFriend(caller, request?.Username),
            GameServiceFaultCode.InvalidInput);

        GameFaultFactory.ThrowIfRejected(status, InviteFriendStatus.Success, MapInviteFaultCode);
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

    private TResult ExecuteForCaller<TResult>(
        Func<PlayerIdentity, TResult> operation,
        GameServiceFaultCode invalidInputFaultCode)
    {
        PlayerIdentity caller = _lobbySessionManager.ResolveCaller();
        TResult result = _faultTranslator.Execute(() => operation(caller), invalidInputFaultCode);
        return result;
    }
}
