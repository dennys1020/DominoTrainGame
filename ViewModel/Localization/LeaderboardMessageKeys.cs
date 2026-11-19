using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class LeaderboardMessageKeys
{
    private const string LeaderboardServerErrorMessageKey = "LeaderboardServerErrorMessage";

    public static string GetMessageKey(LeaderboardStatus status)
    {
        string messageKey;

        switch (status)
        {
            case LeaderboardStatus.SessionNotFound:
                messageKey = CommonMessageKeys.SessionExpiredMessageKey;
                break;
            case LeaderboardStatus.DatabaseUnavailable:
                messageKey = CommonMessageKeys.DatabaseErrorMessageKey;
                break;
            case LeaderboardStatus.ServerUnavailable:
                messageKey = CommonMessageKeys.ServerUnavailableMessageKey;
                break;
            default:
                messageKey = LeaderboardServerErrorMessageKey;
                break;
        }

        return messageKey;
    }
}
