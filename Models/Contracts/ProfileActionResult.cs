namespace DominoTrainGame.Client.Models.Contracts;

public enum ProfileActionResult
{
    Success,
    SessionNotFound,
    GuestNotAllowed,
    PlayerNotFound,
    UsernameAlreadyTaken,
    InvalidInput,
    DatabaseUnavailable,
    AvatarStorageUnavailable,
    ServerUnavailable,
    ServerError
}
