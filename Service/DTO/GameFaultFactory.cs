using System.ServiceModel;

namespace DominoTrainGame.Server.Service.DTO;

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
}
