namespace DominoTrainGame.Server.Application.Results;

public enum SendFriendRequestStatus
{
    Success,
    PlayerNotFound,
    CannotTargetSelf,
    AlreadyFriends,
    RequestAlreadyExists,
    PlayerBlocked
}
