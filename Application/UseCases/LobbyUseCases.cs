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
    private readonly ILobbyCodeGenerator _lobbyCodeGenerator;
    private readonly IChatMessageFilter _chatMessageFilter;
    private readonly IChatSanctionUseCases _chatSanctionUseCases;

    static LobbyUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(LobbyUseCases));
    }

    public LobbyUseCases(
        LobbyRegistry lobbyRegistry,
        IGameNotifier gameNotifier,
        LobbyInputRules inputRules)
    {
        _lobbyRegistry = lobbyRegistry ?? throw new ArgumentNullException(nameof(lobbyRegistry));
        _gameNotifier = gameNotifier ?? throw new ArgumentNullException(nameof(gameNotifier));
        LobbyInputRules rules = inputRules ?? throw new ArgumentNullException(nameof(inputRules));
        _lobbyCodeGenerator = rules.CodeGenerator;
        _chatMessageFilter = rules.MessageFilter;
        _chatSanctionUseCases = rules.ChatSanctions;
    }

    public bool TryCreateLobby(LobbyMember host, out LobbyState lobby)
    {
        bool wasCreated = _lobbyRegistry.TryCreate(host, out lobby);

        if (wasCreated)
        {
            _logger.InfoFormat("A lobby was created. HostPlayerId: {0}.", host.PlayerId);
        }

        return wasCreated;
    }

    public JoinLobbyStatus JoinLobby(LobbyMember member, string code, out LobbyState lobby)
    {
        string normalizedCode = code?.Trim().ToUpperInvariant() ?? string.Empty;
        ValidateCode(normalizedCode);

        JoinLobbyStatus status = _lobbyRegistry.TryJoin(member, normalizedCode, out lobby);

        if (status == JoinLobbyStatus.Success)
        {
            NotifyPlayerJoined(member, lobby);
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

    private void NotifyPlayerJoined(LobbyMember member, LobbyState lobby)
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
                status = SendMessageStatus.Sent;
                break;
        }

        return status;
    }

    private SendMessageStatus SendPermittedMessage(int playerId, string text, LobbyState lobby)
    {
        ChatPermissionStatus permission = _chatSanctionUseCases.CheckChatPermission(playerId);
        SendMessageStatus status = SendMessageStatus.ChatBlocked;

        if (permission == ChatPermissionStatus.Allowed)
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
        SendMessageStatus status = SendMessageStatus.Sent;

        NotifyLobbyMessage(playerId, filteredText, lobby);

        if (wasCensored)
        {
            _logger.InfoFormat("A lobby message was censored. PlayerId: {0}.", playerId);
            status = ToSendMessageStatus(_chatSanctionUseCases.RegisterInfraction(playerId));
        }

        return status;
    }

    private void ValidateCode(string code)
    {
        bool isCodeValid = _lobbyCodeGenerator.IsValid(code);

        if (!isCodeValid)
        {
            throw new ArgumentException("The lobby code format is invalid.", nameof(code));
        }
    }
}
