namespace DominoTrainGame.Client.ViewModel.Validation;

public static class FriendUsernameInputValidator
{
    public const int MaximumUsernameLength = 45;

    private const int EmptyLength = 0;

    public static string Normalize(string username)
    {
        string normalizedUsername = username?.Trim() ?? string.Empty;
        return normalizedUsername;
    }

    public static FriendUsernameInputStatus Validate(string username)
    {
        string normalizedUsername = Normalize(username);
        FriendUsernameInputStatus inputStatus = FriendUsernameInputStatus.Valid;

        if (normalizedUsername.Length == EmptyLength)
        {
            inputStatus = FriendUsernameInputStatus.EmptyUsername;
        }
        else if (normalizedUsername.Length > MaximumUsernameLength)
        {
            inputStatus = FriendUsernameInputStatus.UsernameTooLong;
        }

        return inputStatus;
    }
}
