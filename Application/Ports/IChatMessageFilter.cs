namespace DominoTrainGame.Server.Application.Ports;

public interface IChatMessageFilter
{
    string Filter(string text);
}
