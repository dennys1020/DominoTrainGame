namespace DominoTrainGame.Client.Models.Contracts;

public enum FriendActionResult
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
