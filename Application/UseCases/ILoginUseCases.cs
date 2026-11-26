using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Authenticates the registered players.
/// </summary>
public interface ILoginUseCases
{
    /// <summary>
    /// Checks the username or email and the password of a registered player.
    /// </summary>
    /// <param name="identifier">The username or email, with or without surrounding spaces.</param>
    /// <param name="password">The password entered by the player.</param>
    /// <returns>
    /// The authenticated player, or <see cref="PlayerIdentity.Empty"/> when the credentials are not valid.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="identifier"/> or <paramref name="password"/> is empty or longer than the
    /// maximum length.
    /// </exception>
    PlayerIdentity LogIn(string identifier, string password);
}
