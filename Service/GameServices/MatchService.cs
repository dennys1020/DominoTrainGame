using System;
using System.ServiceModel;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
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

        PlayerIdentity caller = ResolveCaller(request.SessionToken);
        PlayTileStatus status;

        try
        {
            status = _matchUseCases.PlayTile(caller.PlayerId, ToTile(request.Tile), request.TrainOwnerId);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        GameFaultFactory.ThrowIfRejected(status, PlayTileStatus.Success, MapPlayTileFaultCode);
    }

    public void DrawTile(SessionTokenRequest request)
    {
        PlayerIdentity caller = ResolveCaller(request?.SessionToken);
        DrawTileStatus status;

        try
        {
            status = _matchUseCases.DrawTile(caller.PlayerId);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

        GameFaultFactory.ThrowIfRejected(status, DrawTileStatus.Success, MapDrawTileFaultCode);
    }

    public void PassTurn(SessionTokenRequest request)
    {
        PlayerIdentity caller = ResolveCaller(request?.SessionToken);
        PassTurnStatus status;

        try
        {
            status = _matchUseCases.PassTurn(caller.PlayerId);
        }
        catch (ArgumentException exception)
        {
            throw _faultTranslator.TranslateInvalidInput(exception, GameServiceFaultCode.InvalidInput);
        }
        catch (DatabaseUnavailableException exception)
        {
            throw _faultTranslator.TranslateDatabaseUnavailable(exception);
        }

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


    private PlayerIdentity ResolveCaller(string sessionToken)
    {
        PlayerIdentity caller = _sessionToken.Resolve(sessionToken);

        if (caller.IsEmpty)
        {
            throw GameFaultFactory.Create(GameServiceFaultCode.SessionNotFound);
        }

        _sessionToken.Renew(sessionToken, caller);

        return caller;
    }
}
