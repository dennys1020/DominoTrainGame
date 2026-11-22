using System;

namespace DominoTrainGame.Server.Application.Errors;

public sealed class AvatarStorageUnavailableException : Exception
{
    public AvatarStorageUnavailableException()
    {
    }

    public AvatarStorageUnavailableException(string message) : base(message)
    {
    }

    public AvatarStorageUnavailableException(
        string message,
        Exception innerException) : base(message, innerException)
    {
    }
}
