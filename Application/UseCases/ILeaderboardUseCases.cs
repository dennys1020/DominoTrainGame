using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Builds the leaderboard of the registered players.
/// </summary>
public interface ILeaderboardUseCases
{
    /// <summary>
    /// Gets the registered players with the most wins, with their rank.
    /// </summary>
    /// <returns>
    /// A limited list of entries ordered by rank, or an empty list when no match was saved. Guests are not
    /// included.
    /// </returns>
    IReadOnlyList<LeaderboardEntry> GetLeaderboard();
}
