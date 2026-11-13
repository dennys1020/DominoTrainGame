using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public interface ILobbyUseCases
{
    LobbyOutcome<CreateLobbyStatus> CreateLobby(PlayerIdentity host);

    LobbyOutcome<JoinLobbyStatus> JoinLobby(PlayerIdentity member, string code);

    void LeaveLobby(int playerId);

    bool TrySetReady(int playerId, bool isReady);

    SendMessageStatus SendMessage(int playerId, string text);

    StartMatchStatus StartMatch(int playerId);
}
