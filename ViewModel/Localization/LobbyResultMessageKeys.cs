using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class LobbyResultMessageKeys
{
    private const string InvalidLobbyCodeMessageKey = "InvalidLobbyCodeMessage";
    private const string InvalidLobbyInputMessageKey = "InvalidLobbyInputMessage";
    private const string LobbyNotFoundMessageKey = "LobbyNotFoundMessage";
    private const string LobbyFullMessageKey = "LobbyFullMessage";
    private const string LobbyConflictMessageKey = "LobbyConflictMessage";
    private const string LobbyServerErrorMessageKey = "LobbyServerErrorMessage";
    private const string NotLobbyHostMessageKey = "NotLobbyHostMessage";
    private const string PlayersNotReadyMessageKey = "PlayersNotReadyMessage";
    private const string MatchAlreadyStartedMessageKey = "MatchAlreadyStartedMessage";
    private const string FriendPlayerNotFoundMessageKey = "FriendPlayerNotFoundMessage";
    private const string NotFriendsMessageKey = "NotFriendsMessage";
    private const string PlayerOfflineMessageKey = "PlayerOfflineMessage";
    private const string ChatWarningMessageKey = "ChatWarningMessage";
    private const string ChatBlockedMessageKey = "ChatBlockedMessage";
    private const string PlayerBannedMessageKey = "PlayerBannedMessage";

    public static string GetMessageKey(LobbyResultStatus status)
    {
        string messageKey;

        switch (status)
        {
            case LobbyResultStatus.InvalidCode:
                messageKey = InvalidLobbyCodeMessageKey;
                break;
            case LobbyResultStatus.InvalidInput:
                messageKey = InvalidLobbyInputMessageKey;
                break;
            case LobbyResultStatus.LobbyNotFound:
                messageKey = LobbyNotFoundMessageKey;
                break;
            case LobbyResultStatus.LobbyFull:
                messageKey = LobbyFullMessageKey;
                break;
            case LobbyResultStatus.AlreadyInLobby:
                messageKey = LobbyConflictMessageKey;
                break;
            case LobbyResultStatus.SessionNotFound:
                messageKey = CommonMessageKeys.SessionExpiredMessageKey;
                break;
            case LobbyResultStatus.ServerUnavailable:
                messageKey = CommonMessageKeys.ServerUnavailableMessageKey;
                break;
            default:
                messageKey = GetMatchStartMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetMatchStartMessageKey(LobbyResultStatus status)
    {
        string messageKey;

        switch (status)
        {
            case LobbyResultStatus.NotLobbyHost:
                messageKey = NotLobbyHostMessageKey;
                break;
            case LobbyResultStatus.PlayersNotReady:
                messageKey = PlayersNotReadyMessageKey;
                break;
            case LobbyResultStatus.MatchAlreadyStarted:
                messageKey = MatchAlreadyStartedMessageKey;
                break;
            default:
                messageKey = GetInvitationMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetInvitationMessageKey(LobbyResultStatus status)
    {
        string messageKey;

        switch (status)
        {
            case LobbyResultStatus.PlayerNotFound:
                messageKey = FriendPlayerNotFoundMessageKey;
                break;
            case LobbyResultStatus.NotFriends:
                messageKey = NotFriendsMessageKey;
                break;
            case LobbyResultStatus.PlayerOffline:
                messageKey = PlayerOfflineMessageKey;
                break;
            default:
                messageKey = GetChatMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetChatMessageKey(LobbyResultStatus status)
    {
        string messageKey;

        switch (status)
        {
            case LobbyResultStatus.ChatWarning:
                messageKey = ChatWarningMessageKey;
                break;
            case LobbyResultStatus.ChatBlocked:
                messageKey = ChatBlockedMessageKey;
                break;
            case LobbyResultStatus.PlayerBanned:
                messageKey = PlayerBannedMessageKey;
                break;
            default:
                messageKey = LobbyServerErrorMessageKey;
                break;
        }

        return messageKey;
    }
}
