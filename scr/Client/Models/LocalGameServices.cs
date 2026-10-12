using DominoTrainGame.Contracts;
using DominoTrainGame.Server.Bootstrapper;

namespace DominoTrainGame.Client.Models;

// The original application runs its services in the same process.
// Only this adapter knows how to compose the current server implementation.
public static class LocalGameServices
{
    public static IGameServices Create() => ServerComposition.CreateServices();
}
