using System;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace DominoTrainGame.Server.Host;

internal sealed class GameServiceHost : ServiceHost
{
    private readonly IErrorHandler _errorHandler;

    public GameServiceHost(object serviceInstance, IErrorHandler errorHandler) : base(serviceInstance)
    {
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    protected override void InitializeRuntime()
    {
        base.InitializeRuntime();

        foreach (ChannelDispatcherBase dispatcherBase in ChannelDispatchers)
        {
            if (dispatcherBase is ChannelDispatcher channelDispatcher)
            {
                channelDispatcher.ErrorHandlers.Add(_errorHandler);
            }
        }
    }
}
