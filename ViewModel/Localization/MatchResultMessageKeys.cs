using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class MatchResultMessageKeys
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

    public static string GetMessageKey(MatchResultStatus status)
    {
        string messageKey;

        switch (status)
        {
            case MatchResultStatus.MatchNotFound:
                messageKey = MatchNotFoundMessageKey;
                break;
            case MatchResultStatus.NotYourTurn:
                messageKey = NotYourTurnMessageKey;
                break;
            case MatchResultStatus.TileNotInHand:
                messageKey = TileNotInHandMessageKey;
                break;
            case MatchResultStatus.TileDoesNotMatch:
                messageKey = TileDoesNotMatchMessageKey;
                break;
            case MatchResultStatus.TrainNotAvailable:
                messageKey = TrainNotAvailableMessageKey;
                break;
            default:
                messageKey = GetDrawMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetDrawMessageKey(MatchResultStatus status)
    {
        string messageKey;

        switch (status)
        {
            case MatchResultStatus.BoneyardEmpty:
                messageKey = BoneyardEmptyMessageKey;
                break;
            case MatchResultStatus.AlreadyDrawn:
                messageKey = AlreadyDrawnMessageKey;
                break;
            case MatchResultStatus.MustDrawFirst:
                messageKey = MustDrawFirstMessageKey;
                break;
            default:
                messageKey = GetConnectionMessageKey(status);
                break;
        }

        return messageKey;
    }

    private static string GetConnectionMessageKey(MatchResultStatus status)
    {
        string messageKey;

        switch (status)
        {
            case MatchResultStatus.SessionNotFound:
                messageKey = CommonMessageKeys.SessionExpiredMessageKey;
                break;
            case MatchResultStatus.ServerUnavailable:
                messageKey = CommonMessageKeys.ServerUnavailableMessageKey;
                break;
            default:
                messageKey = MatchServerErrorMessageKey;
                break;
        }

        return messageKey;
    }
}
