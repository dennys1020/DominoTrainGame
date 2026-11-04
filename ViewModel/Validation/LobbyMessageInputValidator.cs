namespace DominoTrainGame.Client.ViewModel.Validation;

public static class LobbyMessageInputValidator
{
    public const int MaximumMessageLength = 200;

    private const int EmptyLength = 0;

    public static string Normalize(string text)
    {
        string normalizedText = text?.Trim() ?? string.Empty;
        return normalizedText;
    }

    public static LobbyMessageInputStatus Validate(string text)
    {
        string normalizedText = Normalize(text);
        LobbyMessageInputStatus inputStatus = LobbyMessageInputStatus.Valid;

        if (normalizedText.Length == EmptyLength)
        {
            inputStatus = LobbyMessageInputStatus.EmptyMessage;
        }
        else if (normalizedText.Length > MaximumMessageLength)
        {
            inputStatus = LobbyMessageInputStatus.MessageTooLong;
        }

        return inputStatus;
    }
}
