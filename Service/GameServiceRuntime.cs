using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application;
using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Connection;
using DominoTrainGame.Server.Service.Implementation;
using DominoTrainGame.Server.Service.Networking;

namespace DominoTrainGame.Server.Service;

public sealed class GameServiceRuntime : IDisposable
{
    private static readonly ITraceLog _logger;
    private readonly ServiceHost _host;
    private readonly GameChannelCloser _channelCloser;

    static GameServiceRuntime()
    {
        _logger = TraceLogManager.GetLogger(typeof(GameServiceRuntime));
    }

    public GameServiceRuntime()
    {
        IConnectionUseCases connectionUseCases = UseCaseFactory.CreateConnectionUseCases();
        ILoginUseCases loginUseCases = UseCaseFactory.CreateLoginUseCases();
        ISignUpUseCases signUpUseCases = UseCaseFactory.CreateSignUpUseCases();
        GameService gameService = new GameService(connectionUseCases, loginUseCases,signUpUseCases);

        _channelCloser = new GameChannelCloser();
        _host = new ServiceHost(gameService);
    }

    public void Start()
    {
        _host.Open();

        foreach (ServiceEndpoint endpoint in _host.Description.Endpoints)
        {
            _logger.InfoFormat("Endpoint listening at {0}.", endpoint.Address);
        }
    }

    public void Dispose()
    {
        _channelCloser.Close(_host);
    }
}
