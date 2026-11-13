namespace DominoTrainGame.Client.Models.Contracts;

public enum LobbyActionResult
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
    PlayersNotReady,
    PlayerNotFound,
    NotFriends,
    PlayerOffline,
    ChatWarning,
    ChatBlocked,
    PlayerBanned,
    MatchAlreadyStarted
}
