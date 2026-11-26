using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Reads and creates the accounts of the players.
/// </summary>
public interface IPlayerRepository
{
    /// <summary>
    /// Finds the login data of a registered player by username or email.
    /// </summary>
    /// <param name="identifier">The normalized username or email.</param>
    /// <returns>
    /// The identifier, username and password hash of the player, or <see cref="PlayerCredentials.Empty"/> when no
    /// registered player matches.
    /// </returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    PlayerCredentials FindCredentials(string identifier);

    /// <summary>
    /// Creates a registered player and an empty profile in the same transaction.
    /// </summary>
    /// <param name="registration">The normalized username, the email and the password hash.</param>
    /// <returns>
    /// <c>true</c> when the player was created; <c>false</c> when another player already uses the username
    /// or the email.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="registration"/> is null.
    /// </exception>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    /// <exception cref="Errors.DataIntegrityException">
    /// Thrown when the changes violate a database constraint.
    /// </exception>
    bool CreatePlayer(PlayerRegistration registration);

    /// <summary>
    /// Creates a guest player, which has no email, password or profile.
    /// </summary>
    /// <param name="username">The generated guest username.</param>
    /// <returns>
    /// The new guest, or <see cref="PlayerIdentity.Empty"/> when another player already uses the username.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="username"/> is empty.
    /// </exception>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    /// <exception cref="Errors.DataIntegrityException">
    /// Thrown when the changes violate a database constraint.
    /// </exception>
    PlayerIdentity CreateGuest(string username);

    /// <summary>
    /// Checks whether a player, registered or guest, already uses a username or an email.
    /// </summary>
    /// <param name="username">The normalized username.</param>
    /// <param name="email">The normalized email.</param>
    /// <returns><c>true</c> when the username or the email is in use; otherwise, <c>false</c>.</returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    bool IsRegistrationTaken(string username, string email);
}
