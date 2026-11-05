using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public interface ILobbyUseCases
{
    bool TryCreateLobby(LobbyMember host, out LobbyState lobby);

    JoinLobbyStatus JoinLobby(LobbyMember member, string code, out LobbyState lobby);

    void LeaveLobby(int playerId);

    bool TrySetReady(int playerId, bool isReady);

    bool TrySendMessage(int playerId, string text);

    StartMatchStatus StartMatch(int playerId);
}
