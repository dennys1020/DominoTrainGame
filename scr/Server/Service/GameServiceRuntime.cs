using System;
using System.ServiceModel;
using DominoTrainGame.Server.Service.Implementation;
using DominoTrainGame.Server.Persistence.Repositories;

namespace DominoTrainGame.Server.Service;

public sealed class GameServiceRuntime : IDisposable
{
    private readonly ServiceHost _host;

    public GameServiceRuntime()
    {
        SqlAuthenticationStore store = new SqlAuthenticationStore();
        LeaderboardRepository leaderboard = new LeaderboardRepository();

        GameService service = new GameService(store, leaderboard);

        _host = new ServiceHost(service);
    }

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
