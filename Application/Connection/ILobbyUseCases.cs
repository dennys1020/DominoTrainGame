using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.Connection;

public interface ILobbyUseCases
{
    bool TryCreateLobby(LobbyMember host, out LobbyState lobby);

    JoinLobbyStatus JoinLobby(LobbyMember member, string code, out LobbyState lobby);

    void LeaveLobby(int playerId);
}
