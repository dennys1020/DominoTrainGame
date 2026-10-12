using DominoTrainGame.Contracts;
using DominoTrainGame.Repository;
using DominoTrainGame.Utils;
using DominoTrainGame.Server.Application;
using DominoTrainGame.Server.Infrastructure;

namespace DominoTrainGame.Server.Bootstrapper;

public static class ServerComposition
{
    public static IGameServices CreateServices() =>
        new GameServices(new SqlAuthenticationStore(), new EmailSender(), new LeaderboardRepository());
}
