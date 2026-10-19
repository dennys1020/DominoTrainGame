using System;

namespace DominoTrainGame.Server.Persistence.Errors;

public sealed class DatabaseOperationException : Exception
{
    public DatabaseOperationException()
    {
    }

    public DatabaseOperationException(string message) : base(message)
    {
    }

    public DatabaseOperationException(
        string message,
        Exception innerException) : base(message, innerException)
    {
    }
}
