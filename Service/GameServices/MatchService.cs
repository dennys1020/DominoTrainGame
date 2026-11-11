using System;
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
    private readonly SessionTokenRegistry _sessionToken;
    private readonly ServiceFaultTranslator _faultTranslator;

    public MatchService(
        IMatchUseCases matchUseCases,
        SessionTokenRegistry sessionTokenRegistry,
        ServiceFaultTranslator faultTranslator)
    {
        _matchUseCases = matchUseCases ?? throw new ArgumentNullException(nameof(matchUseCases));
        _sessionToken = sessionTokenRegistry ?? throw new ArgumentNullException(nameof(sessionTokenRegistry));
        _faultTranslator = faultTranslator ?? throw new ArgumentNullException(nameof(faultTranslator));
    }

    public void PlayTile(PlayTileRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        PlayTileStatus status = ExecuteForCaller(
            request.SessionToken,
            caller => _matchUseCases.PlayTile(caller.PlayerId, ToTile(request.Tile), request.TrainOwnerId));

        GameFaultFactory.ThrowIfRejected(status, PlayTileStatus.Success, MapPlayTileFaultCode);
    }

    public void DrawTile(MatchTurnRequest request)
    {
        DrawTileStatus status = ExecuteForCaller(
            request?.SessionToken,
            caller => _matchUseCases.DrawTile(caller.PlayerId));

        GameFaultFactory.ThrowIfRejected(status, DrawTileStatus.Success, MapDrawTileFaultCode);
    }

    public void PassTurn(MatchTurnRequest request)
    {
        PassTurnStatus status = ExecuteForCaller(
            request?.SessionToken,
            caller => _matchUseCases.PassTurn(caller.PlayerId));

        GameFaultFactory.ThrowIfRejected(status, PassTurnStatus.Success, MapPassTurnFaultCode);
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

    private TResult ExecuteForCaller<TResult>(string sessionToken, Func<PlayerIdentity, TResult> operation)
    {
        PlayerIdentity caller = ResolveCaller(sessionToken);
        TResult result = _faultTranslator.Execute(() => operation(caller), GameServiceFaultCode.InvalidInput);

        return result;
    }

    private PlayerIdentity ResolveCaller(string sessionToken)
    {
        bool isValid = _sessionToken.TryResolve(sessionToken, out PlayerIdentity caller);

        if (!isValid)
        {
            throw GameFaultFactory.Create(GameServiceFaultCode.SessionNotFound);
        }

        _sessionToken.Renew(sessionToken, caller);

        return caller;
    }
}
