using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteMatchServices : IRemoteMatchServices
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
    private readonly GameClientCallback _callback;
    private readonly RemoteCallGuard _callGuard;

    static RemoteMatchServices()
    {
        _playTileFailures = CreateFailures(PlayTileOperationName);
        _drawTileFailures = CreateFailures(DrawTileOperationName);
        _passTurnFailures = CreateFailures(PassTurnOperationName);
    }

    public RemoteMatchServices(
        IGameConnectionFactory connectionFactory,
        GameChannelCloser channelCloser,
        ClientSession session,
        GameClientCallback callback)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _channelCloser = channelCloser ?? throw new ArgumentNullException(nameof(channelCloser));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        _callGuard = new RemoteCallGuard();
    }

    public IMatchEvents MatchEvents => _callback;

    public MatchSnapshot CurrentMatch => _session.CurrentMatch;

    public PlayerHand CurrentHand => _session.CurrentHand;

    public Task<MatchResultStatus> PlayTileAsync(int tileId, int trainId)
    {
        List<TileSummary> matchingTiles = FindHandTiles(tileId);
        Task<MatchResultStatus> playing = Task.FromResult(MatchResultStatus.TileNotInHand);

        if (matchingTiles.Any())
        {
            TilePlay tilePlay = new TilePlay(this, matchingTiles.First(), trainId);
            playing = _callGuard.ExecuteAsync(tilePlay.SendAsync, _playTileFailures);
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

    private List<TileSummary> FindHandTiles(int tileId)
    {
        List<TileSummary> matchingTiles = new List<TileSummary>();

        foreach (TileSummary handTile in _session.CurrentHand.Tiles)
        {
            if (handTile.TileId == tileId)
            {
                matchingTiles.Add(handTile);
            }
        }

        return matchingTiles;
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

    private sealed class TilePlay
    {
        private readonly RemoteMatchServices _matchServices;
        private readonly TileSummary _tile;
        private readonly int _trainId;

        public TilePlay(RemoteMatchServices matchServices, TileSummary tile, int trainId)
        {
            _matchServices = matchServices ?? throw new ArgumentNullException(nameof(matchServices));
            _tile = tile ?? throw new ArgumentNullException(nameof(tile));
            _trainId = trainId;
        }

        public Task<MatchResultStatus> SendAsync()
        {
            Task<MatchResultStatus> sending = _matchServices.SendPlayTileAsync(_tile, _trainId);
            return sending;
        }
    }
}
