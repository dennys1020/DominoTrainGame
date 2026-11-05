using System;

namespace DominoTrainGame.Server.Application.Ports;

public interface IClock
{
    DateTime UtcNow { get; }
}
