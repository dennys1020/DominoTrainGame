namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Checks that the database can be reached.
/// </summary>
public interface IConnectionRepository
{
    /// <summary>
    /// Opens a connection to the database.
    /// </summary>
    /// <returns><c>true</c> when the connection was opened; otherwise, <c>false</c>.</returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    bool CanConnect();
}
