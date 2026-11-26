using DominoTrainGame.Shared.Common;
using DominoTrainGame.Shared.Rules;

namespace DominoTrainGame.Client.ViewModel.Validation;

public static class FriendUsernameInputValidator
{
    public static FriendUsernameInputStatus Validate(string username)
    {
        string normalizedUsername = InputText.Normalize(username);
        FriendUsernameInputStatus inputStatus = FriendUsernameInputStatus.Valid;

        if (normalizedUsername.Length == InputText.EmptyLength)
        {
            inputStatus = FriendUsernameInputStatus.EmptyUsername;
        }
        else if (GameRules.IsUsernameTooLong(normalizedUsername))
        {
            inputStatus = FriendUsernameInputStatus.UsernameTooLong;
        }

        return inputStatus;
    }
}
