namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Tells whether a player is connected to the server.
/// </summary>
public interface IPresenceTracker
{
    /// <summary>
    /// Checks whether a player has an open social channel.
    /// </summary>
    /// <param name="playerId">The player to check.</param>
    /// <returns><c>true</c> when the player is connected; otherwise, <c>false</c>.</returns>
    bool IsOnline(int playerId);
}
