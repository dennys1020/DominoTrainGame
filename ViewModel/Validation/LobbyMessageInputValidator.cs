using DominoTrainGame.Shared.Common;
using DominoTrainGame.Shared.Rules;

namespace DominoTrainGame.Client.ViewModel.Validation;

public static class LobbyMessageInputValidator
{
    public static LobbyMessageInputStatus Validate(string text)
    {
        string normalizedText = InputText.Normalize(text);
        LobbyMessageInputStatus inputStatus = LobbyMessageInputStatus.Valid;

        if (normalizedText.Length == InputText.EmptyLength)
        {
            inputStatus = LobbyMessageInputStatus.EmptyMessage;
        }
        else if (GameRules.IsMessageTooLong(normalizedText))
        {
            inputStatus = LobbyMessageInputStatus.MessageTooLong;
        }

        return inputStatus;
    }
}
