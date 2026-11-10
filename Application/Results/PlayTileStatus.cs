namespace DominoTrainGame.Server.Application.Results;

public enum PlayTileStatus
{
    Success,
    NotYourTurn,
    TileNotInHand,
    TrainNotAvailable,
    TileDoesNotMatch
}
