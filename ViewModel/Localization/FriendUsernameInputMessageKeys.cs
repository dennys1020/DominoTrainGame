using DominoTrainGame.Client.ViewModel.Validation;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class FriendUsernameInputMessageKeys
{
    private const string FriendUsernameRequiredMessageKey = "FriendUsernameRequiredMessage";
    private const string FriendUsernameTooLongMessageKey = "FriendUsernameTooLongMessage";

    public static string GetMessageKey(FriendUsernameInputStatus inputStatus)
    {
        string messageKey;

        switch (inputStatus)
        {
            case FriendUsernameInputStatus.UsernameTooLong:
                messageKey = FriendUsernameTooLongMessageKey;
                break;
            default:
                messageKey = FriendUsernameRequiredMessageKey;
                break;
        }

        return messageKey;
    }
}
