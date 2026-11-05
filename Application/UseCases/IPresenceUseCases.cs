using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.UseCases;

public interface IPresenceUseCases
{
    PlayerStatus GetStatus(int playerId);

    bool TryFindLobby(int playerId, out LobbyState lobby);
}
