using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.UseCases;

public interface ILeaderboardUseCases
{
    IReadOnlyList<LeaderboardEntry> GetLeaderboard();
}
