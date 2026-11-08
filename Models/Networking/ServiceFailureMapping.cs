using System;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal sealed class ServiceFailureMapping<TStatus>
{
    private readonly Func<GameServiceFaultCode, TStatus> _mapFault;

    public ServiceFailureMapping(
        string operationName,
        Func<GameServiceFaultCode, TStatus> mapFault,
        TStatus serverError,
        TStatus serverUnavailable)
    {
        OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
        _mapFault = mapFault ?? throw new ArgumentNullException(nameof(mapFault));
        ServerError = serverError;
        ServerUnavailable = serverUnavailable;
    }

    public string OperationName { get; }

    public TStatus ServerError { get; }

    public TStatus ServerUnavailable { get; }

    public TStatus MapFault(GameServiceFaultCode faultCode)
    {
        TStatus status = _mapFault(faultCode);
        return status;
    }
}
