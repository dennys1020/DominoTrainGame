using System;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal sealed class ServiceFailureMapping<TResult>
{
    private readonly Func<GameServiceFaultCode, TResult> _mapFault;

    public ServiceFailureMapping(
        Func<GameServiceFaultCode, TResult> mapFault,
        TResult serverError,
        TResult serverUnavailable)
    {
        _mapFault = mapFault ?? throw new ArgumentNullException(nameof(mapFault));
        ServerError = serverError;
        ServerUnavailable = serverUnavailable;
    }

    public TResult ServerError { get; }

    public TResult ServerUnavailable { get; }

    public TResult MapFault(GameServiceFaultCode faultCode)
    {
        TResult result = _mapFault(faultCode);
        return result;
    }
}
