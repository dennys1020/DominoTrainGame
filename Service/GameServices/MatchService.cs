using System;
using System.Collections.Generic;
using System.ServiceModel;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;
using DominoTrainGame.Server.Service.Sessions;

namespace DominoTrainGame.Server.Service.Implementation;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class MatchService : IMatchService
{
    private readonly IMatchUseCases _matchUseCases;
    private readonly ILeaderboardUseCases _leaderboardUseCases;
    private readonly SessionTokenRegistry _sessionToken;
    private readonly ServiceFaultTranslator _faultTranslator;

    public MatchService(
        IMatchUseCases matchUseCases,
        ILeaderboardUseCases leaderboardUseCases,
        SessionTokenRegistry sessionTokenRegistry,
        ServiceFaultTranslator faultTranslator)
    {
        _matchUseCases = matchUseCases ?? throw new ArgumentNullException(nameof(matchUseCases));
        _leaderboardUseCases = leaderboardUseCases ?? throw new ArgumentNullException(nameof(leaderboardUseCases));
        _sessionToken = sessionTokenRegistry ?? throw new ArgumentNullException(nameof(sessionTokenRegistry));
        _faultTranslator = faultTranslator ?? throw new ArgumentNullException(nameof(faultTranslator));
    }

    public void PlayTile(PlayTileRequest playTileRequest)
    {
        if (playTileRequest == null)
        {
            throw new ArgumentNullException(nameof(playTileRequest));
        }

        PlayerIdentity caller = ResolveCaller(playTileRequest.SessionToken);
        PlayTileStatus status = _faultTranslator.Execute(
            () => _matchUseCases.PlayTile(caller.PlayerId, ToTile(playTileRequest.Tile), playTileRequest.TrainOwnerId));

        GameFaultFactory.ThrowIfRejected(status, PlayTileStatus.Success, MapPlayTileFaultCode);
    }

    public void DrawTile(SessionTokenRequest drawTileRequest)
    {
        PlayerIdentity caller = ResolveCaller(drawTileRequest?.SessionToken);
        DrawTileStatus status = _faultTranslator.Execute(() => _matchUseCases.DrawTile(caller.PlayerId));

        GameFaultFactory.ThrowIfRejected(status, DrawTileStatus.Success, MapDrawTileFaultCode);
    }

    public void PassTurn(SessionTokenRequest passTurnRequest)
    {
        PlayerIdentity caller = ResolveCaller(passTurnRequest?.SessionToken);
        PassTurnStatus status = _faultTranslator.Execute(() => _matchUseCases.PassTurn(caller.PlayerId));

        GameFaultFactory.ThrowIfRejected(status, PassTurnStatus.Success, MapPassTurnFaultCode);
    }

    public void LeaveMatch(SessionTokenRequest leaveMatchRequest)
    {
        PlayerIdentity caller = ResolveCaller(leaveMatchRequest?.SessionToken);
        bool wasRemoved = _faultTranslator.Execute(() => _matchUseCases.LeaveMatch(caller.PlayerId));

        GameFaultFactory.ThrowIfRejected(wasRemoved, GameServiceFaultCode.MatchNotFound);
    }

    public List<LeaderboardEntryData> GetLeaderboard(SessionTokenRequest leaderboardRequest)
    {
        ResolveCaller(leaderboardRequest?.SessionToken);
        IReadOnlyList<LeaderboardEntry> leaderboard = _faultTranslator.Execute(_leaderboardUseCases.GetLeaderboard);
        List<LeaderboardEntryData> leaderboardData = ToLeaderboardData(leaderboard);

        return leaderboardData;
    }

    private static Tile ToTile(TileData tileData)
    {
        if (tileData == null)
        {
            throw new ArgumentNullException(nameof(tileData));
        }

        Tile tile = new Tile(tileData.LeftPips, tileData.RightPips);

        return tile;
    }

    private static List<LeaderboardEntryData> ToLeaderboardData(IReadOnlyList<LeaderboardEntry> leaderboard)
    {
        List<LeaderboardEntryData> leaderboardData = new List<LeaderboardEntryData>();

        foreach (LeaderboardEntry entry in leaderboard)
        {
            leaderboardData.Add(new LeaderboardEntryData
            {
                Rank = entry.Rank,
                Username = entry.Statistics.Username,
                Wins = entry.Statistics.Wins,
                GamesPlayed = entry.Statistics.GamesPlayed
            });
        }

        return leaderboardData;
    }

    private static GameServiceFaultCode MapPlayTileFaultCode(PlayTileStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case PlayTileStatus.MatchNotFound:
                faultCode = GameServiceFaultCode.MatchNotFound;
                break;
            case PlayTileStatus.NotYourTurn:
                faultCode = GameServiceFaultCode.NotYourTurn;
                break;
            case PlayTileStatus.TileNotInHand:
                faultCode = GameServiceFaultCode.TileNotInHand;
                break;
            case PlayTileStatus.TileDoesNotMatch:
                faultCode = GameServiceFaultCode.TileDoesNotMatch;
                break;
            case PlayTileStatus.TrainNotAvailable:
                faultCode = GameServiceFaultCode.TrainNotAvailable;
                break;
            case PlayTileStatus.MustSatisfyDouble:
                faultCode = GameServiceFaultCode.MustSatisfyDouble;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidInput;
                break;
        }

        return faultCode;
    }

    private static GameServiceFaultCode MapDrawTileFaultCode(DrawTileStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case DrawTileStatus.MatchNotFound:
                faultCode = GameServiceFaultCode.MatchNotFound;
                break;
            case DrawTileStatus.NotYourTurn:
                faultCode = GameServiceFaultCode.NotYourTurn;
                break;
            case DrawTileStatus.BoneyardEmpty:
                faultCode = GameServiceFaultCode.BoneyardEmpty;
                break;
            case DrawTileStatus.AlreadyDrewTile:
                faultCode = GameServiceFaultCode.AlreadyDrawn;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidInput;
                break;
        }

        return faultCode;
    }

    private static GameServiceFaultCode MapPassTurnFaultCode(PassTurnStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case PassTurnStatus.MatchNotFound:
                faultCode = GameServiceFaultCode.MatchNotFound;
                break;
            case PassTurnStatus.NotYourTurn:
                faultCode = GameServiceFaultCode.NotYourTurn;
                break;
            case PassTurnStatus.MustDrawFirst:
                faultCode = GameServiceFaultCode.MustDrawFirst;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidInput;
                break;
        }

        return faultCode;
    }

    private PlayerIdentity ResolveCaller(string sessionToken)
    {
        PlayerIdentity caller = _sessionToken.Resolve(sessionToken);

        if (caller.IsEmpty)
        {
            throw GameFaultFactory.Create(GameServiceFaultCode.SessionNotFound);
        }

        _sessionToken.Renew(sessionToken);

        return caller;
    }
}
