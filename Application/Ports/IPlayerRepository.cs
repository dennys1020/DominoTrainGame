namespace DominoTrainGame.Server.Application.Ports;

public interface IPlayerRepository
{
    bool TryFindCredentials(string identifier, out PlayerCredentials credentials);

    bool CreatePlayer(PlayerRegistration registration);
}
