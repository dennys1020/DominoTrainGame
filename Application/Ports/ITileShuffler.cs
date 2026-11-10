using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public interface ITileShuffler
{
    IReadOnlyList<Tile> Shuffle(IReadOnlyList<Tile> tiles);
}
