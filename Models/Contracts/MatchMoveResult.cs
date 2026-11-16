namespace DominoTrainGame.Client.Models.Contracts;

public enum MatchMoveResult
{
    Success,
    MatchNotFound,
    NotYourTurn,
    TileNotInHand,
    TileDoesNotMatch,
    TrainNotAvailable,
    MustSatisfyDouble,
    BoneyardEmpty,
    AlreadyDrawn,
    MustDrawFirst,
    SessionNotFound,
    ServerUnavailable,
    ServerError
}
