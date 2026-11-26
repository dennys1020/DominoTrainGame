namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Creates and checks the codes that players use to join a lobby.
/// </summary>
public interface ILobbyCodeGenerator
{
    /// <summary>
    /// Creates a random lobby code.
    /// </summary>
    /// <returns>
    /// A code of six uppercase letters and digits, without characters that look alike, such as <c>O</c> and
    /// <c>0</c>.
    /// </returns>
    string Generate();

    /// <summary>
    /// Checks that a code has the length and the characters of a generated code.
    /// </summary>
    /// <param name="code">The trimmed, uppercase code to check.</param>
    /// <returns><c>true</c> when the code has the generated format; otherwise, <c>false</c>.</returns>
    bool IsValid(string code);
}
