using System;

namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Provides the current time, so the time can be replaced in the tests.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    /// <value>The current date and time in Coordinated Universal Time.</value>
    DateTime UtcNow { get; }
}
