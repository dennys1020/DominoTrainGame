using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class MatchMessageKeys
{
    private const string MatchNotFoundMessageKey = "MatchNotFoundMessage";
    private const string NotYourTurnMessageKey = "MatchNotYourTurnMessage";
    private const string TileNotInHandMessageKey = "MatchTileNotInHandMessage";
    private const string TileDoesNotMatchMessageKey = "MatchTileDoesNotMatchMessage";
    private const string TrainNotAvailableMessageKey = "MatchTrainNotAvailableMessage";
    private const string BoneyardEmptyMessageKey = "MatchBoneyardEmptyMessage";
    private const string AlreadyDrawnMessageKey = "MatchAlreadyDrawnMessage";
    private const string MustDrawFirstMessageKey = "MatchMustDrawFirstMessage";
    private const string MatchServerErrorMessageKey = "MatchServerErrorMessage";

    public static string GetMessageKey(MatchMoveResult status)
    {
        string messageKey;

        switch (status)
        {
            case MatchMoveResult.MatchNotFound:
                messageKey = MatchNotFoundMessageKey;
                break;
            case MatchMoveResult.NotYourTurn:
                messageKey = NotYourTurnMessageKey;
                break;
            case MatchMoveResult.TileNotInHand:
                messageKey = TileNotInHandMessageKey;
                break;
            case MatchMoveResult.TileDoesNotMatch:
                messageKey = TileDoesNotMatchMessageKey;
                break;
            case MatchMoveResult.TrainNotAvailable:
                messageKey = TrainNotAvailableMessageKey;
                break;
            default:
                messageKey = GetDrawMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetDrawMessageKey(MatchMoveResult status)
    {
        string messageKey;

        switch (status)
        {
            case MatchMoveResult.BoneyardEmpty:
                messageKey = BoneyardEmptyMessageKey;
                break;
            case MatchMoveResult.AlreadyDrawn:
                messageKey = AlreadyDrawnMessageKey;
                break;
            case MatchMoveResult.MustDrawFirst:
                messageKey = MustDrawFirstMessageKey;
                break;
            default:
                messageKey = GetConnectionMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetConnectionMessageKey(MatchMoveResult status)
    {
        string messageKey;

        switch (status)
        {
            case MatchMoveResult.SessionNotFound:
                messageKey = CommonMessageKeys.SessionExpiredMessageKey;
                break;
            case MatchMoveResult.ServerUnavailable:
                messageKey = CommonMessageKeys.ServerUnavailableMessageKey;
                break;
            default:
                messageKey = MatchServerErrorMessageKey;
                break;
        }

        return messageKey;
    }
}
