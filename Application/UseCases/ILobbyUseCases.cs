using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public interface ILobbyUseCases
{
    CreateLobbyStatus CreateLobby(PlayerIdentity host, out LobbyState lobby);

    JoinLobbyStatus JoinLobby(PlayerIdentity member, string code, out LobbyState lobby);

    void LeaveLobby(int playerId);

    bool TrySetReady(int playerId, bool isReady);

    SendMessageStatus SendMessage(int playerId, string text);

    StartMatchStatus StartMatch(int playerId);
}
