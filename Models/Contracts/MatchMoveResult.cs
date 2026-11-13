namespace DominoTrainGame.Client.Models.Contracts;

public enum MatchMoveResult
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
