namespace DominoTrainGame.Client.Models.Contracts;

public enum LobbyResultStatus
{
    Success,
    InvalidCode,
    InvalidInput,
    LobbyNotFound,
    LobbyFull,
    AlreadyInLobby,
    AlreadyConnected,
    SessionNotFound,
    ServerUnavailable,
    ServerError,
    NotLobbyHost,
    PlayersNotReady
}
