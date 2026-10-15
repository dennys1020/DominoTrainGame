using System;
using System.ServiceModel;
using DominoTrainGame.Server.Application;
using DominoTrainGame.Server.Service.Implementation;

namespace DominoTrainGame.Server.Service;

// The console host only manages this service's lifecycle.
public sealed class GameServiceRuntime : IDisposable
{
    private readonly ServiceHost _host;

    public GameServiceRuntime() =>
        _host = new ServiceHost(new GameService(ApplicationComposition.CreateUseCases()));

    public void Start() => _host.Open();

    public void Dispose()
    {
        try
        {
            if (_host.State == CommunicationState.Faulted) { _host.Abort(); }
            else { _host.Close(); }
        }
        catch (CommunicationException) { _host.Abort(); }
        catch (TimeoutException) { _host.Abort(); }
    }
}
