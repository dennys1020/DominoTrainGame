using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public interface IChatSanctionUseCases
{
    bool CanChat(int playerId);

    ChatInfractionStatus RegisterInfraction(int playerId);

    bool IsBannedFromPlaying(int playerId);
}
