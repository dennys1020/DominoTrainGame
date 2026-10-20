using System;

namespace DominoTrainGame.Server.Application.Errors;

public sealed class DatabaseUnavailableException : Exception
{
    public DatabaseUnavailableException()
    {
    }

    public DatabaseUnavailableException(string message) : base(message)
    {
    }

    public DatabaseUnavailableException(
        string message,
        Exception innerException) : base(message, innerException)
    {
    }
}
