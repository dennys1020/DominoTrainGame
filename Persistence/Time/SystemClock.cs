using System;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Persistence.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
