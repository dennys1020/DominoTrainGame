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

    public static string GetMessageKey(LobbyActionResult status)
    {
        string messageKey;

        switch (status)
        {
            case LobbyActionResult.InvalidCode:
                messageKey = InvalidLobbyCodeMessageKey;
                break;
            case LobbyActionResult.InvalidInput:
                messageKey = InvalidLobbyInputMessageKey;
                break;
            case LobbyActionResult.LobbyNotFound:
                messageKey = LobbyNotFoundMessageKey;
                break;
            case LobbyActionResult.LobbyFull:
                messageKey = LobbyFullMessageKey;
                break;
            case LobbyActionResult.AlreadyInLobby:
                messageKey = LobbyConflictMessageKey;
                break;
            case LobbyActionResult.SessionNotFound:
                messageKey = CommonMessageKeys.SessionExpiredMessageKey;
                break;
            case LobbyActionResult.ServerUnavailable:
                messageKey = CommonMessageKeys.ServerUnavailableMessageKey;
                break;
            default:
                messageKey = GetMatchStartMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetMatchStartMessageKey(LobbyActionResult status)
    {
        string messageKey;

        switch (status)
        {
            case LobbyActionResult.NotLobbyHost:
                messageKey = NotLobbyHostMessageKey;
                break;
            case LobbyActionResult.PlayersNotReady:
                messageKey = PlayersNotReadyMessageKey;
                break;
            case LobbyActionResult.MatchAlreadyStarted:
                messageKey = MatchAlreadyStartedMessageKey;
                break;
            default:
                messageKey = GetInvitationMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetInvitationMessageKey(LobbyActionResult status)
    {
        string messageKey;

        switch (status)
        {
            case LobbyActionResult.PlayerNotFound:
                messageKey = FriendPlayerNotFoundMessageKey;
                break;
            case LobbyActionResult.NotFriends:
                messageKey = NotFriendsMessageKey;
                break;
            case LobbyActionResult.PlayerOffline:
                messageKey = PlayerOfflineMessageKey;
                break;
            default:
                messageKey = GetChatMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetChatMessageKey(LobbyActionResult status)
    {
        string messageKey;

        switch (status)
        {
            case LobbyActionResult.ChatWarning:
                messageKey = ChatWarningMessageKey;
                break;
            case LobbyActionResult.ChatBlocked:
                messageKey = ChatBlockedMessageKey;
                break;
            case LobbyActionResult.PlayerBanned:
                messageKey = PlayerBannedMessageKey;
                break;
            default:
                messageKey = LobbyServerErrorMessageKey;
                break;
        }

        return messageKey;
    }
}
