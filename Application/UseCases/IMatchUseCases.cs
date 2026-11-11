using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public interface IMatchUseCases
{
    bool StartMatch(string code, IReadOnlyList<int> playerIds);

    PlayTileStatus PlayTile(int playerId, Tile tile, int trainOwnerId);

    DrawTileStatus DrawTile(int playerId);

    PassTurnStatus PassTurn(int playerId);
}
