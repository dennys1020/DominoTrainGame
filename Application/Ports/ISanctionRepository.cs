using System;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Stores the chat mutes and game bans of the players.
/// </summary>
public interface ISanctionRepository
{
    /// <summary>
    /// Finds the sanctions of a player that are still active.
    /// </summary>
    /// <param name="playerId">The player whose sanctions are read.</param>
    /// <param name="now">The current time in Coordinated Universal Time.</param>
    /// <returns>
    /// One sanction that ends with the last active sanction and is a game ban when any active sanction is a
    /// game ban, or <see cref="PlayerSanction.Empty"/> when the player has no active sanctions.
    /// </returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    PlayerSanction FindActiveSanction(int playerId, DateTime now);

    /// <summary>
    /// Counts every chat mute and game ban that a player ever received, active or not.
    /// </summary>
    /// <param name="playerId">The player whose sanctions are counted.</param>
    /// <returns>The number of sanctions of the player.</returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    int CountSanctions(int playerId);

    /// <summary>
    /// Saves a new sanction of a player.
    /// </summary>
    /// <param name="sanctionRecord">The player, the type and end of the sanction, and when it started.</param>
    /// <returns><c>true</c> when the sanction was saved; otherwise, <c>false</c>.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="sanctionRecord"/> is null.
    /// </exception>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    /// <exception cref="Errors.DataIntegrityException">
    /// Thrown when the changes violate a database constraint.
    /// </exception>
    bool AddSanction(SanctionRecord sanctionRecord);
}
