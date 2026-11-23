using System;

namespace DominoTrainGame.Server.Application.Ports;

public interface IGracePeriodScheduler
{
    void Start(int playerId, TimeSpan gracePeriod, Action<int> expire);

    bool Cancel(int playerId);
}
