using System;

namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Keeps the timers of the players that lost the connection during a match.
/// </summary>
public interface IGracePeriodScheduler
{
    /// <summary>
    /// Starts the grace period of a player and replaces any grace period that the player already had.
    /// </summary>
    /// <param name="playerId">The player that lost the connection.</param>
    /// <param name="gracePeriod">The time the player has to come back.</param>
    /// <param name="expire">
    /// The action that runs with <paramref name="playerId"/> when the time ends before a call to
    /// <see cref="Cancel"/>. It runs on a timer thread.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="expire"/> is null.
    /// </exception>
    void Start(int playerId, TimeSpan gracePeriod, Action<int> expire);

    /// <summary>
    /// Stops the grace period of a player, so its expire action never runs.
    /// </summary>
    /// <param name="playerId">The player that came back.</param>
    /// <returns><c>true</c> when a grace period was pending; otherwise, <c>false</c>.</returns>
    bool Cancel(int playerId);
}
