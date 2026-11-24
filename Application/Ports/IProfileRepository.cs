using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Reads and updates the profiles of registered players. A player without a profile is a guest.
/// </summary>
public interface IProfileRepository
{
    /// <summary>
    /// Finds the profile of the registered player with the given username.
    /// </summary>
    /// <param name="username">The normalized username of the player.</param>
    /// <returns>
    /// The profile of the player, or <see cref="PlayerProfileSummary.Empty"/> when no registered player
    /// has that username.
    /// </returns>
    PlayerProfileSummary FindProfileByUsername(string username);

    /// <summary>
    /// Finds the profile of the player with the given identifier.
    /// </summary>
    /// <param name="playerId">The player whose profile is read.</param>
    /// <returns>
    /// The profile of the player, or <see cref="PlayerProfileSummary.Empty"/> when the player is a guest.
    /// </returns>
    PlayerProfileSummary FindProfileByPlayerId(int playerId);

    /// <summary>
    /// Replaces the biography of a player.
    /// </summary>
    /// <param name="playerId">The player whose biography changes.</param>
    /// <param name="biography">The normalized biography.</param>
    /// <returns><c>true</c> when the biography was saved; <c>false</c> when the player is a guest.</returns>
    bool UpdateBiography(int playerId, string biography);

    /// <summary>
    /// Replaces the three social network usernames of a player.
    /// </summary>
    /// <param name="playerId">The player whose social links change.</param>
    /// <param name="socialLinks">The normalized social network usernames.</param>
    /// <returns><c>true</c> when the links were saved; <c>false</c> when the player is a guest.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="socialLinks"/> is null.
    /// </exception>
    bool UpdateSocialLinks(int playerId, SocialLinks socialLinks);

    /// <summary>
    /// Replaces the name of the avatar file of a player.
    /// </summary>
    /// <param name="playerId">The player whose avatar changes.</param>
    /// <param name="avatarFileName">The name of the stored avatar file.</param>
    /// <returns><c>true</c> when the name was saved; <c>false</c> when the player is a guest.</returns>
    bool UpdateAvatarFileName(int playerId, string avatarFileName);

    /// <summary>
    /// Changes the username of a player when no other player uses it.
    /// </summary>
    /// <param name="playerId">The player whose username changes.</param>
    /// <param name="newUsername">The normalized new username.</param>
    /// <returns>The result of the change, or the reason it was rejected.</returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="newUsername"/> is empty.
    /// </exception>
    ChangeUsernameStatus ChangeUsername(int playerId, string newUsername);
}
