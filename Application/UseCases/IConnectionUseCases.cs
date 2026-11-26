namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Checks that the server can work with the database.
/// </summary>
public interface IConnectionUseCases
{
    /// <summary>
    /// Opens a connection to the database to check that it is available.
    /// </summary>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the connection cannot be opened.
    /// </exception>
    void VerifyDatabaseConnection();
}
