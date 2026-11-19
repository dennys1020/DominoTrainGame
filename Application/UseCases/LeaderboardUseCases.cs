using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class LeaderboardUseCases : ILeaderboardUseCases
{
    private const int MaximumLeaderboardPlayers = 50;

    private readonly IMatchResultRepository _matchResultRepository;

    public LeaderboardUseCases(IMatchResultRepository matchResultRepository)
    {
        _matchResultRepository = matchResultRepository
            ?? throw new ArgumentNullException(nameof(matchResultRepository));
    }

    public IReadOnlyList<LeaderboardEntry> GetLeaderboard()
    {
        IReadOnlyList<PlayerStatistics> statisticsByMostWins =
            _matchResultRepository.GetStatisticsByMostWins(MaximumLeaderboardPlayers);
        IReadOnlyList<LeaderboardEntry> leaderboard = MatchRanking.RankByMostWins(statisticsByMostWins);

        return leaderboard;
    }
}
