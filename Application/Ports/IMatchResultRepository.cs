namespace DominoTrainGame.Server.Application.Ports;

public interface IMatchResultRepository
{
    bool SaveMatchResults(FinishedMatchRecord finishedMatch);
}
