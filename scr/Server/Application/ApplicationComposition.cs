using DominoTrainGame.Server.Application.Notifications;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Persistence.Repositories;

namespace DominoTrainGame.Server.Application;

public static class ApplicationComposition
{
    public static GameUseCases CreateUseCases() =>
        new GameUseCases(new SqlAuthenticationStore(), new EmailSender(), new LeaderboardRepository());
}
