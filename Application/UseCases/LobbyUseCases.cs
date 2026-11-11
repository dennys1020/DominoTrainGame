using System;
using System.Collections.Generic;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.State;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class LobbyUseCases : ILobbyUseCases
{
    private const int MaximumMessageLength = 200;

    private static readonly ITraceLog _logger;

    private readonly LobbyRegistry _lobbyRegistry;
    private readonly IGameNotifier _gameNotifier;
    private readonly IChatMessageFilter _chatMessageFilter;
    private readonly IChatSanctionUseCases _chatSanctionUseCases;
    private readonly IFriendUseCases _friendUseCases;
    private readonly IMatchUseCases _matchUseCases;

    static LobbyUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(LobbyUseCases));
    }

    public LobbyUseCases(
        LobbyRegistry lobbyRegistry,
        IGameNotifier gameNotifier,
        IChatMessageFilter chatMessageFilter,
        IChatSanctionUseCases chatSanctionUseCases,
        IFriendUseCases friendUseCases,
        IMatchUseCases matchUseCases)
    {
        _lobbyRegistry = lobbyRegistry ?? throw new ArgumentNullException(nameof(lobbyRegistry));
        _gameNotifier = gameNotifier ?? throw new ArgumentNullException(nameof(gameNotifier));
        _chatMessageFilter = chatMessageFilter ?? throw new ArgumentNullException(nameof(chatMessageFilter));
        _chatSanctionUseCases = chatSanctionUseCases
            ?? throw new ArgumentNullException(nameof(chatSanctionUseCases));
        _friendUseCases = friendUseCases ?? throw new ArgumentNullException(nameof(friendUseCases));
        _matchUseCases = matchUseCases ?? throw new ArgumentNullException(nameof(matchUseCases));
    }

    public CreateLobbyStatus CreateLobby(PlayerIdentity host, out LobbyState lobby)
    {
        if (host == null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        lobby = LobbyState.Empty;
        CreateLobbyStatus status = CreateLobbyStatus.PlayerBanned;
        bool isBanned = _chatSanctionUseCases.IsBannedFromPlaying(host.PlayerId);

        if (!isBanned)
        {
            bool wasCreated = _lobbyRegistry.TryCreate(host, out lobby);
            status = wasCreated ? CreateLobbyStatus.Success : CreateLobbyStatus.AlreadyInLobby;
        }

        if (status == CreateLobbyStatus.Success)
        {
            _logger.InfoFormat("A lobby was created. HostPlayerId: {0}.", host.PlayerId);
            _friendUseCases.PublishStatus(host.PlayerId);
        }
        else
        {
            _logger.InfoFormat("A lobby creation was rejected. PlayerId: {0}. Status: {1}.", host.PlayerId, status);
        }

        return status;
    }

    public JoinLobbyStatus JoinLobby(PlayerIdentity member, string code, out LobbyState lobby)
    {
        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        string normalizedCode = _lobbyRegistry.NormalizeCode(code);
        lobby = LobbyState.Empty;
        JoinLobbyStatus status = JoinLobbyStatus.PlayerBanned;
        bool isBanned = _chatSanctionUseCases.IsBannedFromPlaying(member.PlayerId);

        if (!isBanned)
        {
            status = _lobbyRegistry.TryJoin(member, normalizedCode, out lobby);
        }

        if (status == JoinLobbyStatus.Success)
        {
            NotifyPlayerJoined(member, lobby);
            _friendUseCases.PublishStatus(member.PlayerId);
        }
        else
        {
            _logger.InfoFormat("A join request was rejected. PlayerId: {0}. Status: {1}.", member.PlayerId, status);
        }

        return status;
    }

    public void LeaveLobby(int playerId)
    {
        bool wasInLobby = _lobbyRegistry.Remove(playerId, out LobbyState remainingLobby);

        if (wasInLobby)
        {
            NotifyPlayerLeft(playerId, remainingLobby);
            _friendUseCases.PublishStatus(playerId);
        }
    }

    public bool TrySetReady(int playerId, bool isReady)
    {
        bool wasUpdated = _lobbyRegistry.TrySetReady(playerId, isReady, out LobbyState lobby);

        if (wasUpdated)
        {
            NotifyPlayerReadyChanged(playerId, lobby);
        }
        else
        {
            _logger.WarnFormat(
                "A ready state change was rejected because the player is not in a lobby. PlayerId: {0}.",
                playerId);
        }

        return wasUpdated;
    }

    public SendMessageStatus SendMessage(int playerId, string text)
    {
        string normalizedText = text?.Trim() ?? string.Empty;
        ValidateMessageText(normalizedText);

        bool isInLobby = _lobbyRegistry.TryFindLobbyOfPlayer(playerId, out LobbyState lobby);
        SendMessageStatus status = SendMessageStatus.NotInLobby;

        if (isInLobby)
        {
            status = SendPermittedMessage(playerId, normalizedText, lobby);
        }
        else
        {
            _logger.WarnFormat(
                "A lobby message was rejected because the player is not in a lobby. PlayerId: {0}.",
                playerId);
        }

        return status;
    }

    public StartMatchStatus StartMatch(int playerId)
    {
        bool isInLobby = _lobbyRegistry.TryFindLobbyOfPlayer(playerId, out LobbyState lobby);
        StartMatchStatus status = isInLobby ? GetStartStatus(playerId, lobby) : StartMatchStatus.LobbyNotFound;

        if (status == StartMatchStatus.Success)
        {
            //TODO agregar la validacion por si un jugador hace doble click al querer iniciar una partida
            _matchUseCases.StartMatch(lobby.Code, lobby.Roster.GetPlayerIds());
            NotifyMatchStarting(lobby);
        }
        else
        {
            _logger.WarnFormat("A match start was rejected. PlayerId: {0}. Status: {1}.", playerId, status);
        }

        return status;
    }

    private static StartMatchStatus GetStartStatus(int playerId, LobbyState lobby)
    {
        StartMatchStatus status = StartMatchStatus.Success;
        bool isHost = lobby.Roster.HostPlayerId == playerId;
        bool canStart = lobby.HasMinimumPlayers && lobby.Roster.AreAllPlayersReady;

        if (!isHost)
        {
            status = StartMatchStatus.NotLobbyHost;
        }
        else if (!canStart)
        {
            status = StartMatchStatus.PlayersNotReady;
        }

        return status;
    }

    private static void ValidateMessageText(string text)
    {
        bool isEmpty = string.IsNullOrEmpty(text);
        bool isTooLong = text.Length > MaximumMessageLength;

        if (isEmpty || isTooLong)
        {
            throw new ArgumentException(
                "The lobby message is empty or longer than the allowed length.",
                nameof(text));
        }
    }

    private static SendMessageStatus ToSendMessageStatus(ChatInfractionStatus infraction)
    {
        SendMessageStatus status;

        switch (infraction)
        {
            case ChatInfractionStatus.Warned:
                status = SendMessageStatus.Warned;
                break;
            case ChatInfractionStatus.ChatBlocked:
            case ChatInfractionStatus.GameBanned:
                status = SendMessageStatus.ChatBlocked;
                break;
            default:
                status = SendMessageStatus.Success;
                break;
        }

        return status;
    }

    private void NotifyPlayerJoined(PlayerIdentity member, LobbyState lobby)
    {
        List<int> recipientPlayerIds = lobby.Roster.GetPlayerIdsExcept(member.PlayerId);
        PlayerJoinedNotice notice = new PlayerJoinedNotice(recipientPlayerIds, member, lobby.Roster.Version);

        _logger.InfoFormat("A player joined a lobby. PlayerId: {0}.", member.PlayerId);
        _gameNotifier.NotifyPlayerJoined(notice);
    }

    private void NotifyPlayerLeft(int playerId, LobbyState remainingLobby)
    {
        LobbyRoster roster = remainingLobby.Roster;
        List<int> recipientPlayerIds = roster.GetPlayerIdsExcept(playerId);
        PlayerLeftNotice notice = new PlayerLeftNotice(recipientPlayerIds, playerId, roster);

        _logger.InfoFormat("A player left a lobby. PlayerId: {0}.", playerId);
        _gameNotifier.NotifyPlayerLeft(notice);
    }

    private void NotifyPlayerReadyChanged(int playerId, LobbyState lobby)
    {
        LobbyRoster roster = lobby.Roster;
        bool wasFound = roster.TryFindPlayer(playerId, out LobbyPlayerState player);

        if (wasFound)
        {
            PlayerReadyChangedNotice notice = new PlayerReadyChangedNotice(
                roster.GetPlayerIds(),
                player,
                roster.Version);

            _logger.InfoFormat(
                "A player changed the ready state. PlayerId: {0}. IsReady: {1}.",
                playerId,
                player.IsReady);
            _gameNotifier.NotifyPlayerReadyChanged(notice);
        }
    }

    private void NotifyLobbyMessage(int playerId, string text, LobbyState lobby)
    {
        LobbyRoster roster = lobby.Roster;
        bool wasFound = roster.TryFindPlayer(playerId, out LobbyPlayerState sender);

        if (wasFound)
        {
            LobbyMessageNotice notice = new LobbyMessageNotice(roster.GetPlayerIds(), sender.Member, text);

            _logger.DebugFormat("A player sent a lobby message. PlayerId: {0}.", playerId);
            _gameNotifier.NotifyLobbyMessage(notice);
        }
    }

    private void NotifyMatchStarting(LobbyState lobby)
    {
        MatchStartingNotice notice = new MatchStartingNotice(lobby.Roster.GetPlayerIds(), lobby.Code);

        _logger.InfoFormat("A match is starting. LobbyCode: {0}.", lobby.Code);
        _gameNotifier.NotifyMatchStarting(notice);
    }

    private SendMessageStatus SendPermittedMessage(int playerId, string text, LobbyState lobby)
    {
        bool canChat = _chatSanctionUseCases.CanChat(playerId);
        SendMessageStatus status = SendMessageStatus.ChatBlocked;

        if (canChat)
        {
            status = SendFilteredMessage(playerId, text, lobby);
        }
        else
        {
            _logger.WarnFormat(
                "A lobby message was rejected because the player chat is blocked. PlayerId: {0}.",
                playerId);
        }

        return status;
    }

    private SendMessageStatus SendFilteredMessage(int playerId, string text, LobbyState lobby)
    {
        string filteredText = _chatMessageFilter.Filter(text);
        bool wasCensored = !string.Equals(filteredText, text, StringComparison.Ordinal);
        SendMessageStatus status = SendMessageStatus.Success;

        NotifyLobbyMessage(playerId, filteredText, lobby);

        if (wasCensored)
        {
            _logger.InfoFormat("A lobby message was censored. PlayerId: {0}.", playerId);
            status = ToSendMessageStatus(_chatSanctionUseCases.RegisterInfraction(playerId));
        }

        return status;
    }
}
