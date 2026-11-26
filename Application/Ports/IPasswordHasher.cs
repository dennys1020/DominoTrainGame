namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Hashes the passwords before they are stored and checks the passwords entered at login.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password with a new random salt.
    /// </summary>
    /// <param name="password">The password entered by the player.</param>
    /// <returns>The format marker, the salt and the derived key, encoded in Base64.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="password"/> is null.
    /// </exception>
    string Hash(string password);

    /// <summary>
    /// Checks a password against a stored hash in constant time.
    /// </summary>
    /// <param name="hashedPassword">The hash returned by <see cref="Hash"/>.</param>
    /// <param name="password">The password entered by the player.</param>
    /// <returns>
    /// <c>true</c> when the password matches; <c>false</c> when it does not match, when either value is
    /// empty or when the hash does not have the supported format.
    /// </returns>
    bool Verify(string hashedPassword, string password);
}
