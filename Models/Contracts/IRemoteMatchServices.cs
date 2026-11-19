using System.Collections.Generic;
using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IRemoteMatchServices
{
    IMatchEvents MatchEvents { get; }

    MatchSnapshot CurrentMatch { get; }

    PlayerHand CurrentHand { get; }

    Task<MatchMoveResult> PlayTileAsync(int tileId, int trainId);

    Task<MatchMoveResult> DrawTileAsync();

    Task<MatchMoveResult> PassTurnAsync();

    Task<MatchMoveResult> LeaveMatchAsync();

    Task<LeaderboardResult> GetLeaderboardAsync();

    IReadOnlyList<RoundSummary> RoundHistory { get; }
}
