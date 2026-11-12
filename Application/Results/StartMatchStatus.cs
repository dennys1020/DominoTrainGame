namespace DominoTrainGame.Server.Application.Results;

public enum StartMatchStatus
{
    Success,
    LobbyNotFound,
    NotLobbyHost,
    PlayersNotReady,
    MatchAlreadyStarted
}
