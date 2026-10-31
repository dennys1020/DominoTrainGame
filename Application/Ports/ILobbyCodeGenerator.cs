namespace DominoTrainGame.Server.Application.Ports;

public interface ILobbyCodeGenerator
{
    string Generate();

    bool IsValid(string code);
}
