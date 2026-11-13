using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.State;

public sealed class ChatInfractionRegistry
{
    private const int SingleInfraction = 1;

    private readonly ConcurrentDictionary<int, int> _infractionsByPlayerId;

    public ChatInfractionRegistry()
    {
        _infractionsByPlayerId = new ConcurrentDictionary<int, int>();
    }

    public int Add(int playerId)
    {
        int infractionCount = _infractionsByPlayerId.AddOrUpdate(
            playerId,
            SingleInfraction,
            (existingPlayerId, currentCount) => currentCount + SingleInfraction);
        return infractionCount;
    }

    public void Reset(int playerId)
    {
        IDictionary<int, int> infractionsByPlayerId = _infractionsByPlayerId;
        infractionsByPlayerId.Remove(playerId);
    }
}
