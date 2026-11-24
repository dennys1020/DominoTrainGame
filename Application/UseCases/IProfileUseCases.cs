using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Reads and edits the profiles of registered players.
/// </summary>
public interface IProfileUseCases
{
    /// <summary>
    /// Finds the profile of a registered player by username.
    /// </summary>
    /// <param name="username">The username to search, with or without surrounding spaces.</param>
    /// <returns>
    /// The profile of the player, or <see cref="PlayerProfileSummary.Empty"/> when no registered player
    /// has that username.
    /// </returns>
    PlayerProfileSummary FindProfileByUsername(string username);

    /// <summary>
    /// Gets the games played, wins and total points of a player.
    /// </summary>
    /// <param name="playerId">The player whose statistics are read.</param>
    /// <returns>The statistics of the player, or empty statistics when the player has no matches.</returns>
    PlayerProfileStatistics GetProfileStatistics(int playerId);

    /// <summary>
    /// Loads the avatar image of a profile.
    /// </summary>
    /// <param name="profile">The profile whose avatar is loaded.</param>
    /// <returns>
    /// The content of the image, or an empty array when the profile has no avatar or the file cannot be read.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="profile"/> is null.
    /// </exception>
    byte[] LoadProfileAvatarImage(PlayerProfileSummary profile);

    /// <summary>
    /// Replaces the biography of a player.
    /// </summary>
    /// <param name="playerId">The player whose biography changes.</param>
    /// <param name="biography">The new biography.</param>
    /// <returns><c>true</c> when the biography was saved; <c>false</c> when the player is a guest.</returns>
    bool UpdateProfileBiography(int playerId, string biography);

    /// <summary>
    /// Changes the username of a player.
    /// </summary>
    /// <param name="playerId">The player whose username changes.</param>
    /// <param name="newUsername">The new username.</param>
    /// <returns>The result of the change, or the reason it was rejected.</returns>
    ChangeUsernameStatus ChangeProfileUsername(int playerId, string newUsername);

    /// <summary>
    /// Replaces the social network usernames of a player.
    /// </summary>
    /// <param name="playerId">The player whose social links change.</param>
    /// <param name="socialLinks">The new social network usernames. An empty username removes that network.</param>
    /// <returns><c>true</c> when the links were saved; <c>false</c> when the player is a guest.</returns>
    bool UpdateProfileSocialLinks(int playerId, SocialLinks socialLinks);

    /// <summary>
    /// Replaces the avatar image of a player and deletes the previous file.
    /// </summary>
    /// <param name="playerId">The player whose avatar changes.</param>
    /// <param name="imageBytes">The content of a PNG or JPG image of at most 1 MB.</param>
    /// <returns><c>true</c> when the avatar was saved; <c>false</c> when the player is a guest.</returns>
    bool UpdateProfileAvatar(int playerId, byte[] imageBytes);
}
