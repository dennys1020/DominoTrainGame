using System.Collections.Generic;
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
    private const string SessionExpiredMessageKey = "LobbySessionExpiredMessage";
    private const string DatabaseErrorMessageKey = "DatabaseErrorMessage";
    private const string ServerUnavailableMessageKey = "ServerUnavailableMessage";
    private const string ServerErrorMessageKey = "FriendServerErrorMessage";

    private static readonly IReadOnlyDictionary<FriendResultStatus, string> _messageKeysByStatus;

    static FriendResultMessageKeys()
    {
        _messageKeysByStatus = new Dictionary<FriendResultStatus, string>
        {
            [FriendResultStatus.InvalidInput] = InvalidInputMessageKey,
            [FriendResultStatus.PlayerNotFound] = PlayerNotFoundMessageKey,
            [FriendResultStatus.CannotTargetSelf] = CannotTargetSelfMessageKey,
            [FriendResultStatus.AlreadyFriends] = AlreadyFriendsMessageKey,
            [FriendResultStatus.RequestAlreadyExists] = RequestAlreadyExistsMessageKey,
            [FriendResultStatus.PlayerBlocked] = PlayerBlockedMessageKey,
            [FriendResultStatus.RequestNotFound] = RequestNotFoundMessageKey,
            [FriendResultStatus.NotFriends] = NotFriendsMessageKey,
            [FriendResultStatus.SessionNotFound] = SessionExpiredMessageKey,
            [FriendResultStatus.DatabaseUnavailable] = DatabaseErrorMessageKey,
            [FriendResultStatus.ServerUnavailable] = ServerUnavailableMessageKey
        };
    }

    public static string GetMessageKey(FriendResultStatus status)
    {
        bool hasMessageKey = _messageKeysByStatus.TryGetValue(status, out string knownMessageKey);
        string messageKey = hasMessageKey ? knownMessageKey : ServerErrorMessageKey;
        return messageKey;
    }
}
