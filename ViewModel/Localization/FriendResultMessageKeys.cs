using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class FriendResultMessageKeys
{
    private const string InvalidInputMessageKey = "FriendInvalidInputMessage";
    private const string PlayerNotFoundMessageKey = "FriendPlayerNotFoundMessage";
    private const string CannotTargetSelfMessageKey = "FriendCannotTargetSelfMessage";
    private const string AlreadyFriendsMessageKey = "AlreadyFriendsMessage";
    private const string RequestAlreadyExistsMessageKey = "FriendRequestAlreadyExistsMessage";
    private const string PlayerBlockedMessageKey = "FriendPlayerBlockedMessage";
    private const string RequestNotFoundMessageKey = "FriendRequestNotFoundMessage";
    private const string NotFriendsMessageKey = "NotFriendsMessage";
    private const string ServerErrorMessageKey = "FriendServerErrorMessage";

    public static string GetMessageKey(FriendResultStatus status)
    {
        string messageKey;

        switch (status)
        {
            case FriendResultStatus.InvalidInput:
                messageKey = InvalidInputMessageKey;
                break;
            case FriendResultStatus.SessionNotFound:
                messageKey = CommonMessageKeys.SessionExpiredMessageKey;
                break;
            case FriendResultStatus.DatabaseUnavailable:
                messageKey = CommonMessageKeys.DatabaseErrorMessageKey;
                break;
            case FriendResultStatus.ServerUnavailable:
                messageKey = CommonMessageKeys.ServerUnavailableMessageKey;
                break;
            default:
                messageKey = GetFriendshipMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetFriendshipMessageKey(FriendResultStatus status)
    {
        string messageKey;

        switch (status)
        {
            case FriendResultStatus.PlayerNotFound:
                messageKey = PlayerNotFoundMessageKey;
                break;
            case FriendResultStatus.CannotTargetSelf:
                messageKey = CannotTargetSelfMessageKey;
                break;
            case FriendResultStatus.AlreadyFriends:
                messageKey = AlreadyFriendsMessageKey;
                break;
            case FriendResultStatus.RequestAlreadyExists:
                messageKey = RequestAlreadyExistsMessageKey;
                break;
            case FriendResultStatus.PlayerBlocked:
                messageKey = PlayerBlockedMessageKey;
                break;
            case FriendResultStatus.RequestNotFound:
                messageKey = RequestNotFoundMessageKey;
                break;
            case FriendResultStatus.NotFriends:
                messageKey = NotFriendsMessageKey;
                break;
            default:
                messageKey = ServerErrorMessageKey;
                break;
        }

        return messageKey;
    }
}
