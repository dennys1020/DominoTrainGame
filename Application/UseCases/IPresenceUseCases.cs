using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Tells the status that the friends of a player see.
/// </summary>
public interface IPresenceUseCases
{
    /// <summary>
    /// Gets the status of a player.
    /// </summary>
    /// <param name="playerId">The player to check.</param>
    /// <returns>
    /// <see cref="PlayerStatus.InLobby"/> when the player is in a lobby, <see cref="PlayerStatus.Online"/>
    /// when the player is connected, or <see cref="PlayerStatus.Offline"/>.
    /// </returns>
    PlayerStatus GetStatus(int playerId);
}
