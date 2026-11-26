namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Keeps the seat of a player that lost the connection during a match.
/// </summary>
public interface IReconnectionUseCases
{
    /// <summary>
    /// Keeps the seat and the hand of a disconnected player during the grace period.
    /// </summary>
    /// <remarks>
    /// When the grace period ends before the player comes back, the player leaves the match.
    /// </remarks>
    /// <param name="playerId">The player that lost the connection.</param>
    /// <returns><c>true</c> when the player was in a match that is not finished; otherwise, <c>false</c>.</returns>
    bool HoldSeat(int playerId);

    /// <summary>
    /// Gives a player back the seat that was kept and sends the current state of the match.
    /// </summary>
    /// <param name="playerId">The player that comes back.</param>
    /// <param name="matchCode">The code of the match that the player had.</param>
    /// <returns>
    /// <c>true</c> when the player returned to the match; <c>false</c> when the code does not match or the
    /// grace period already ended.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="matchCode"/> is null or empty.
    /// </exception>
    bool Reconnect(int playerId, string matchCode);
}
