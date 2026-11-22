using System;

namespace DominoTrainGame.Server.Application.Errors;

public sealed class DataIntegrityException : Exception
{
    public DataIntegrityException()
    {
    }

    public DataIntegrityException(string message) : base(message)
    {
    }

    public DataIntegrityException(
        string message,
        Exception innerException) : base(message, innerException)
    {
    }
}
