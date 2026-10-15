using System;

namespace DominoTrainGame.Client.Models.Errors;

public sealed class GameServiceException : Exception
{
    public GameServiceException(string message, Exception innerException = null) : base(message, innerException) { }
}
