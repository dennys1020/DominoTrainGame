namespace DominoTrainGame.Client.Models.Contracts;

public enum LobbyResultStatus
{
    Success,
    InvalidCode,
    LobbyNotFound,
    LobbyFull,
    AlreadyInLobby,
    AlreadyConnected,
    SessionNotFound,
    ServerUnavailable,
    ServerError
}
