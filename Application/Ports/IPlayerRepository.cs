namespace DominoTrainGame.Server.Application.Ports;

public interface IPlayerRepository
{
    PlayerCredentials FindCredentials(string identifier);

    bool CreatePlayer(PlayerRegistration registration);
}
