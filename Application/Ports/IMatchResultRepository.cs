using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public interface IMatchResultRepository
{
    bool SaveMatchResults(FinishedMatchRecord finishedMatch);

    IReadOnlyList<PlayerStatistics> GetStatisticsByMostWins(int maximumPlayers);
}
