namespace DominoTrainGame.Client.Models.Contracts;

public enum MatchResultStatus
{
    Success,
    MatchNotFound,
    NotYourTurn,
    TileNotInHand,
    TileDoesNotMatch,
    TrainNotAvailable,
    BoneyardEmpty,
    AlreadyDrawn,
    MustDrawFirst,
    SessionNotFound,
    ServerUnavailable,
    ServerError
}
