namespace DominoTrainGame.Server.Application.Results;

public enum JoinLobbyStatus
{
    Success,
    LobbyNotFound,
    LobbyFull,
    AlreadyInLobby,
    PlayerBanned
}
