using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IRemoteMatchServices
{
    IMatchEvents MatchEvents { get; }

    MatchSnapshot CurrentMatch { get; }

    PlayerHand CurrentHand { get; }

    Task<MatchResultStatus> PlayTileAsync(int tileId, int trainId);

    Task<MatchResultStatus> DrawTileAsync();

    Task<MatchResultStatus> PassTurnAsync();
}
