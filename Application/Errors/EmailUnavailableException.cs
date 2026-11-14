using System;

namespace DominoTrainGame.Server.Application.Errors;

public sealed class EmailUnavailableException : Exception
{
    public EmailUnavailableException()
    {
    }

    public EmailUnavailableException(string message) : base (message)
    {
    }

    public EmailUnavailableException(
    string message,
    Exception innerException) : base(message, innerException)
    {
    }

}
