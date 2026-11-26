using System;
using System.Collections.Generic;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.State;
using DominoTrainGame.Shared.Common;
using DominoTrainGame.Shared.Rules;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class LobbyUseCases : ILobbyUseCases
{

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

    public LobbyOutcome<CreateLobbyStatus> CreateLobby(PlayerIdentity host)
    {
        if (host == null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        LobbyState lobby = LobbyState.Empty;
        CreateLobbyStatus status = CreateLobbyStatus.PlayerBanned;
        bool isBanned = _chatSanctionUseCases.IsBannedFromPlaying(host.PlayerId);

        if (!isBanned)
        {
            lobby = _lobbyRegistry.Create(host);
            status = lobby.IsEmpty ? CreateLobbyStatus.AlreadyInLobby : CreateLobbyStatus.Success;
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

        LobbyOutcome<CreateLobbyStatus> lobbyCreation = new LobbyOutcome<CreateLobbyStatus>(status, lobby);
        return lobbyCreation;
    }

    public LobbyOutcome<JoinLobbyStatus> JoinLobby(PlayerIdentity member, string code)
    {
        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        string normalizedCode = _lobbyRegistry.NormalizeCode(code);
        LobbyOutcome<JoinLobbyStatus> lobbyJoin = new LobbyOutcome<JoinLobbyStatus>(
            JoinLobbyStatus.PlayerBanned,
            LobbyState.Empty);
        bool isBanned = _chatSanctionUseCases.IsBannedFromPlaying(member.PlayerId);

        if (!isBanned)
        {
            lobbyJoin = _lobbyRegistry.Join(member, normalizedCode);
        }

        if (lobbyJoin.Status == JoinLobbyStatus.Success)
        {
            NotifyPlayerJoined(member, lobbyJoin.Lobby);
            _friendUseCases.PublishStatus(member.PlayerId);
        }
        else
        {
            _logger.InfoFormat(
                "A join request was rejected. PlayerId: {0}. Status: {1}.",
                member.PlayerId,
                lobbyJoin.Status);
        }

        return lobbyJoin;
    }

    public void LeaveLobby(int playerId)
    {
        LobbyState remainingLobby = _lobbyRegistry.RemovePlayer(playerId);

        if (!remainingLobby.IsEmpty)
        {
            NotifyPlayerLeft(playerId, remainingLobby);
            _friendUseCases.PublishStatus(playerId);
        }
    }

    public bool TrySetReady(int playerId, bool isReady)
    {
        LobbyState lobby = _lobbyRegistry.SetReady(playerId, isReady);
        bool wasUpdated = !lobby.IsEmpty;

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

    public SelectDominoSetStatus SelectDominoSet(int playerId, int highestDouble)
    {
        DominoSet dominoSet = DominoSet.FromHighestDouble(highestDouble);
        LobbyOutcome<SelectDominoSetStatus> dominoSetSelection = _lobbyRegistry.SelectDominoSet(playerId, dominoSet);

        if (dominoSetSelection.Status == SelectDominoSetStatus.Success)
        {
            NotifyDominoSetChanged(playerId, dominoSetSelection.Lobby);
        }
        else
        {
            _logger.WarnFormat(
                "A domino set selection was rejected. PlayerId: {0}. Status: {1}.",
                playerId,
                dominoSetSelection.Status);
        }

        return dominoSetSelection.Status;
    }

    public SendMessageStatus SendMessage(int playerId, string text)
    {
        string normalizedText = InputText.Normalize(text);
        ValidateMessageText(normalizedText);

        LobbyState lobby = _lobbyRegistry.FindLobbyOfPlayer(playerId);
        bool isInLobby = !lobby.IsEmpty;
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
        LobbyState lobby = _lobbyRegistry.FindLobbyOfPlayer(playerId);
        StartMatchStatus status = lobby.IsEmpty ? StartMatchStatus.LobbyNotFound : GetStartStatus(playerId, lobby);

        if (status == StartMatchStatus.Success)
        {
            bool wasStarted = _matchUseCases.StartMatch(lobby.Code, lobby.Roster.GetPlayerIds(), lobby.DominoSet);
            status = wasStarted ? StartMatchStatus.Success : StartMatchStatus.MatchAlreadyStarted;
        }

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

    private static void ValidateMessageText(string text)
    {
        bool isEmpty = text.Length == InputText.EmptyLength;
        bool isTooLong = GameRules.IsMessageTooLong(text);

        if (isEmpty || isTooLong)
        {
            _logger.Warn("A lobby message that is empty or longer than the allowed length was rejected.");
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
        LobbyPlayerState player = roster.FindPlayer(playerId);

        if (!player.IsEmpty)
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

    private void NotifyDominoSetChanged(int playerId, LobbyState lobby)
    {
        LobbyRoster roster = lobby.Roster;
        DominoSetChangedNotice notice = new DominoSetChangedNotice(
            roster.GetPlayerIds(),
            lobby.DominoSet,
            roster.Version);

        _logger.InfoFormat(
            "The host changed the domino set. PlayerId: {0}. HighestDouble: {1}.",
            playerId,
            lobby.DominoSet.HighestDouble);
        _gameNotifier.NotifyDominoSetChanged(notice);
    }

    private void NotifyLobbyMessage(int playerId, string text, LobbyState lobby)
    {
        LobbyRoster roster = lobby.Roster;
        LobbyPlayerState sender = roster.FindPlayer(playerId);

        if (!sender.IsEmpty)
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
