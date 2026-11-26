using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class FriendMessageKeys
{
    private const string InvalidInputMessageKey = "FriendInvalidInputMessage";
    private const string CannotTargetSelfMessageKey = "FriendCannotTargetSelfMessage";
    private const string AlreadyFriendsMessageKey = "AlreadyFriendsMessage";
    private const string RequestAlreadyExistsMessageKey = "FriendRequestAlreadyExistsMessage";
    private const string PlayerBlockedMessageKey = "FriendPlayerBlockedMessage";
    private const string RequestNotFoundMessageKey = "FriendRequestNotFoundMessage";
    private const string ServerErrorMessageKey = "FriendServerErrorMessage";

    public static string GetMessageKey(FriendActionResult status)
    {
        string messageKey;

        switch (status)
        {
            case FriendActionResult.InvalidInput:
                messageKey = InvalidInputMessageKey;
                break;
            case FriendActionResult.SessionNotFound:
                messageKey = CommonMessageKeys.SessionExpiredMessageKey;
                break;
            case FriendActionResult.DatabaseUnavailable:
                messageKey = CommonMessageKeys.DatabaseErrorMessageKey;
                break;
            case FriendActionResult.ServerUnavailable:
                messageKey = CommonMessageKeys.ServerUnavailableMessageKey;
                break;
            case FriendActionResult.GuestNotAllowed:
                messageKey = CommonMessageKeys.GuestNotAllowedMessageKey;
                break;
            default:
                messageKey = GetFriendshipMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetFriendshipMessageKey(FriendActionResult status)
    {
        string messageKey;

        switch (status)
        {
            case FriendActionResult.PlayerNotFound:
                messageKey = CommonMessageKeys.FriendPlayerNotFoundMessageKey;
                break;
            case FriendActionResult.CannotTargetSelf:
                messageKey = CannotTargetSelfMessageKey;
                break;
            case FriendActionResult.AlreadyFriends:
                messageKey = AlreadyFriendsMessageKey;
                break;
            case FriendActionResult.RequestAlreadyExists:
                messageKey = RequestAlreadyExistsMessageKey;
                break;
            case FriendActionResult.PlayerBlocked:
                messageKey = PlayerBlockedMessageKey;
                break;
            case FriendActionResult.RequestNotFound:
                messageKey = RequestNotFoundMessageKey;
                break;
            case FriendActionResult.NotFriends:
                messageKey = CommonMessageKeys.NotFriendsMessageKey;
                break;
            default:
                messageKey = ServerErrorMessageKey;
                break;
        }

        return messageKey;
    }
}
