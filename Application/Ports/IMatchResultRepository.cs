using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Stores the results of finished matches and reads the statistics calculated from them.
/// </summary>
public interface IMatchResultRepository
{
    /// <summary>
    /// Saves the final rank and score of every player of a finished match.
    /// </summary>
    /// <param name="finishedMatch">The lobby code and the results of each player.</param>
    /// <returns><c>true</c> when the results were saved; otherwise, <c>false</c>.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="finishedMatch"/> is null.
    /// </exception>
    bool SaveMatchResults(FinishedMatchRecord finishedMatch);

    /// <summary>
    /// Gets the registered players with the most wins, ordered for the leaderboard.
    /// </summary>
    /// <param name="maximumPlayers">The maximum number of players to return.</param>
    /// <returns>The statistics of each player, or an empty list when no matches were saved.</returns>
    IReadOnlyList<PlayerStatistics> GetStatisticsByMostWins(int maximumPlayers);

    /// <summary>
    /// Gets the games played, wins and total points of a registered player.
    /// </summary>
    /// <param name="playerId">The player whose statistics are read.</param>
    /// <returns>
    /// The statistics of the player, or <see cref="PlayerProfileStatistics.Empty"/> when the player
    /// has no finished matches or is a guest.
    /// </returns>
    PlayerProfileStatistics GetProfileStatisticsByPlayerId(int playerId);
}
