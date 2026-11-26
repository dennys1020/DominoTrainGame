namespace DominoTrainGame.Client.ViewModel.Validation;

public static class LobbyMessageInputValidator
{
    public const int MaximumMessageLength = 200;

    public static LobbyMessageInputStatus Validate(string text)
    {
        string normalizedText = InputText.Normalize(text);
        LobbyMessageInputStatus inputStatus = LobbyMessageInputStatus.Valid;

        if (normalizedText.Length == InputText.EmptyLength)
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
