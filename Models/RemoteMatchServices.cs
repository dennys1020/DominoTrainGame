using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.Networking;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteMatchServices : IRemoteMatchServices
{
    private readonly MatchRequests _matchRequests;
    private readonly ClientSession _session;
    private readonly GameClientCallback _callback;

    public RemoteMatchServices(MatchRequests matchRequests, ClientSession session, GameClientCallback callback)
    {
        _matchRequests = matchRequests ?? throw new ArgumentNullException(nameof(matchRequests));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public IMatchEvents MatchEvents => _callback;

    public MatchSnapshot CurrentMatch => _session.CurrentMatch;

    public PlayerHand CurrentHand => _session.CurrentHand;

    public Task<MatchResultStatus> PlayTileAsync(int tileId, int trainId)
    {
        Task<MatchResultStatus> playing = _matchRequests.PlayTileAsync(tileId, trainId);
        return playing;
    }

    public Task<MatchResultStatus> DrawTileAsync()
    {
        Task<MatchResultStatus> drawing = _matchRequests.DrawTileAsync();
        return drawing;
    }

    public Task<MatchResultStatus> PassTurnAsync()
    {
        Task<MatchResultStatus> passing = _matchRequests.PassTurnAsync();
        return passing;
    }
}
