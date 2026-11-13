using System;
using System.Linq;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class MatchRequests
{
    private const string PlayTileOperationName = "play tile";
    private const string DrawTileOperationName = "draw tile";
    private const string PassTurnOperationName = "pass turn";

    private static readonly ServiceFailureMapping<MatchResultStatus> _playTileFailures;
    private static readonly ServiceFailureMapping<MatchResultStatus> _drawTileFailures;
    private static readonly ServiceFailureMapping<MatchResultStatus> _passTurnFailures;

    private readonly IGameConnectionFactory _connectionFactory;
    private readonly GameChannelCloser _channelCloser;
    private readonly ClientSession _session;
    private readonly RemoteCallGuard _callGuard;

    static MatchRequests()
    {
        _playTileFailures = CreateFailures(PlayTileOperationName);
        _drawTileFailures = CreateFailures(DrawTileOperationName);
        _passTurnFailures = CreateFailures(PassTurnOperationName);
    }

    public MatchRequests(
        IGameConnectionFactory connectionFactory,
        GameChannelCloser channelCloser,
        ClientSession session)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _channelCloser = channelCloser ?? throw new ArgumentNullException(nameof(channelCloser));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _callGuard = new RemoteCallGuard();
    }

    public Task<MatchResultStatus> PlayTileAsync(int tileId, int trainId)
    {
        TileSummary tile = _session.CurrentHand.Tiles.FirstOrDefault(handTile => handTile.TileId == tileId);
        Task<MatchResultStatus> playing = Task.FromResult(MatchResultStatus.TileNotInHand);

        if (tile != null)
        {
            playing = _callGuard.ExecuteAsync(() => SendPlayTileAsync(tile, trainId), _playTileFailures);
        }

        return playing;
    }

    public Task<MatchResultStatus> DrawTileAsync()
    {
        Task<MatchResultStatus> drawing = _callGuard.ExecuteAsync(SendDrawTileAsync, _drawTileFailures);
        return drawing;
    }

    public Task<MatchResultStatus> PassTurnAsync()
    {
        Task<MatchResultStatus> passing = _callGuard.ExecuteAsync(SendPassTurnAsync, _passTurnFailures);
        return passing;
    }

    private static ServiceFailureMapping<MatchResultStatus> CreateFailures(string operationName)
    {
        ServiceFailureMapping<MatchResultStatus> failures = new ServiceFailureMapping<MatchResultStatus>(
            operationName,
            FaultCodeMapper.MapMatchFault,
            MatchResultStatus.ServerError,
            MatchResultStatus.ServerUnavailable);
        return failures;
    }

    private async Task<MatchResultStatus> SendPlayTileAsync(TileSummary tile, int trainId)
    {
        MatchServiceClient matchClient = _connectionFactory.CreateMatchClient();
        MatchResultStatus resultStatus;

        try
        {
            PlayTileRequest request = new PlayTileRequest
            {
                SessionToken = _session.SessionToken,
                Tile = new TileData
                {
                    LeftPips = tile.LeftPips,
                    RightPips = tile.RightPips
                },
                TrainOwnerId = trainId
            };

            await matchClient.PlayTileAsync(request).ConfigureAwait(false);
            resultStatus = MatchResultStatus.Success;
        }
        finally
        {
            _channelCloser.Close(matchClient);
        }

        return resultStatus;
    }

    private async Task<MatchResultStatus> SendDrawTileAsync()
    {
        MatchServiceClient matchClient = _connectionFactory.CreateMatchClient();
        MatchResultStatus resultStatus;

        try
        {
            await matchClient.DrawTileAsync(CreateSessionTokenRequest()).ConfigureAwait(false);
            resultStatus = MatchResultStatus.Success;
        }
        finally
        {
            _channelCloser.Close(matchClient);
        }

        return resultStatus;
    }

    private async Task<MatchResultStatus> SendPassTurnAsync()
    {
        MatchServiceClient matchClient = _connectionFactory.CreateMatchClient();
        MatchResultStatus resultStatus;

        try
        {
            await matchClient.PassTurnAsync(CreateSessionTokenRequest()).ConfigureAwait(false);
            resultStatus = MatchResultStatus.Success;
        }
        finally
        {
            _channelCloser.Close(matchClient);
        }

        return resultStatus;
    }

    private SessionTokenRequest CreateSessionTokenRequest()
    {
        SessionTokenRequest request = new SessionTokenRequest
        {
            SessionToken = _session.SessionToken
        };
        return request;
    }
}
