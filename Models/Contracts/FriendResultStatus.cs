namespace DominoTrainGame.Client.Models.Contracts;

public enum FriendResultStatus
{
    Success,
    InvalidInput,
    PlayerNotFound,
    CannotTargetSelf,
    AlreadyFriends,
    RequestAlreadyExists,
    PlayerBlocked,
    RequestNotFound,
    NotFriends,
    SessionNotFound,
    DatabaseUnavailable,
    ServerUnavailable,
    ServerError
}
