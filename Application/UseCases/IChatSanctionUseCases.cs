using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Applies the chat warnings, mutes and game bans of the players that write offensive messages.
/// </summary>
public interface IChatSanctionUseCases
{
    /// <summary>
    /// Checks whether a player can send chat messages.
    /// </summary>
    /// <param name="playerId">The player that wants to chat.</param>
    /// <returns><c>true</c> when the player has no active sanction; otherwise, <c>false</c>.</returns>
    bool CanChat(int playerId);

    /// <summary>
    /// Counts a censored message of a player and applies the next sanction when the player reaches the limit.
    /// </summary>
    /// <remarks>
    /// The first two sanctions mute the chat for a limited time, and the next ones are temporary game bans.
    /// The infraction count starts again after each sanction.
    /// </remarks>
    /// <param name="playerId">The player that sent the censored message.</param>
    /// <returns>
    /// <see cref="ChatInfractionStatus.Success"/> while the player is under the warning limit, or the
    /// warning or sanction that the player received.
    /// </returns>
    ChatInfractionStatus RegisterInfraction(int playerId);

    /// <summary>
    /// Checks whether a player has an active game ban, which blocks creating or joining a lobby.
    /// </summary>
    /// <param name="playerId">The player to check.</param>
    /// <returns><c>true</c> when the player has an active game ban; otherwise, <c>false</c>.</returns>
    bool IsBannedFromPlaying(int playerId);
}
