using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class LobbyResultMessageKeys
{
    private const string InvalidLobbyCodeMessageKey = "InvalidLobbyCodeMessage";
    private const string InvalidLobbyInputMessageKey = "InvalidLobbyInputMessage";
    private const string LobbyNotFoundMessageKey = "LobbyNotFoundMessage";
    private const string LobbyFullMessageKey = "LobbyFullMessage";
    private const string LobbyConflictMessageKey = "LobbyConflictMessage";
    private const string LobbySessionExpiredMessageKey = "LobbySessionExpiredMessage";
    private const string ServerUnavailableMessageKey = "ServerUnavailableMessage";
    private const string LobbyServerErrorMessageKey = "LobbyServerErrorMessage";

    public static string GetMessageKey(LobbyResultStatus status)
    {
        string messageKey;

        switch (status)
        {
            case LobbyResultStatus.InvalidCode:
                messageKey = InvalidLobbyCodeMessageKey;
                break;
            case LobbyResultStatus.InvalidInput:
                messageKey = InvalidLobbyInputMessageKey;
                break;
            case LobbyResultStatus.LobbyNotFound:
                messageKey = LobbyNotFoundMessageKey;
                break;
            case LobbyResultStatus.LobbyFull:
                messageKey = LobbyFullMessageKey;
                break;
            case LobbyResultStatus.AlreadyInLobby:
                messageKey = LobbyConflictMessageKey;
                break;
            case LobbyResultStatus.SessionNotFound:
                messageKey = LobbySessionExpiredMessageKey;
                break;
            case LobbyResultStatus.ServerUnavailable:
                messageKey = ServerUnavailableMessageKey;
                break;
            default:
                messageKey = LobbyServerErrorMessageKey;
                break;
        }

        return messageKey;
    }
}
