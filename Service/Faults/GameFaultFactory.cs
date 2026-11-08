using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DominoTrainGame.Server.Service.Faults;

internal static class GameFaultFactory
{
    public static FaultException<GameServiceFault> Create(GameServiceFaultCode faultCode)
    {
        GameServiceFault fault = new GameServiceFault
        {
            Code = faultCode
        };
        FaultException<GameServiceFault> faultException = new FaultException<GameServiceFault>(fault);

        return faultException;
    }

    public static void ThrowIfRejected<TStatus>(
        TStatus status,
        TStatus successStatus,
        Func<TStatus, GameServiceFaultCode> mapFaultCode)
    {
        if (mapFaultCode == null)
        {
            throw new ArgumentNullException(nameof(mapFaultCode));
        }

        bool isRejected = !EqualityComparer<TStatus>.Default.Equals(status, successStatus);

        if (isRejected)
        {
            throw Create(mapFaultCode(status));
        }
    }

    public static void ThrowIfRejected(bool wasApplied, GameServiceFaultCode faultCode)
    {
        if (!wasApplied)
        {
            throw Create(faultCode);
        }
    }
}
