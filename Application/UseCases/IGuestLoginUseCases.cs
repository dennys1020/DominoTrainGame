using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.UseCases;

public interface IGuestLoginUseCases
{
    PlayerIdentity LogInAsGuest();
}
