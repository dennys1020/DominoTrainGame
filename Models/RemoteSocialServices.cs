using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteSocialServices : IRemoteSocialServices
{
    private const string FriendsOperationName = "friends";

    private static readonly ITraceLog _logger;
    private static readonly ServiceFailureMapping<FriendActionResult> _friendFailures;

    private readonly IGameConnectionFactory _connectionFactory;
    private readonly IRemoteGameServices _gameServices;
    private readonly SocialClientCallback _callback;
    private readonly DuplexClientSlot<FriendServiceClient> _friendClientSlot;
    private readonly RemoteCallGuard _callGuard;

    static RemoteSocialServices()
    {
        _logger = TraceLogManager.GetLogger(typeof(RemoteSocialServices));
        _friendFailures = new ServiceFailureMapping<FriendActionResult>(
            FriendsOperationName,
            FriendResultMapper.MapFault,
            FriendActionResult.ServerError,
            FriendActionResult.ServerUnavailable);
    }

    public RemoteSocialServices(
        IGameConnectionFactory connectionFactory,
        GameChannelCloser channelCloser,
        IRemoteGameServices gameServices,
        SocialClientCallback callback)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        _friendClientSlot = new DuplexClientSlot<FriendServiceClient>(channelCloser);
        _callGuard = new RemoteCallGuard();
        _friendClientSlot.ClientEnded += HandleFriendClientEnded;
    }

    public IFriendEvents FriendEvents => _callback;

    public Task<FriendActionResult> ConnectAsync()
    {
        Task<FriendActionResult> connection = InvokeFriendServiceAsync(
            friendClient => Task.FromResult(FriendActionResult.Success),
            RemoteCallGuard.KeepStatus<FriendActionResult>);
        return connection;
    }

    public Task<FriendListResult> GetFriendsAsync()
    {
        Task<FriendListResult> request = InvokeFriendServiceAsync(
            RequestFriendsAsync,
            FriendResultMapper.CreateFailedFriendList);
        return request;
    }

    public Task<PendingFriendshipListResult> GetPendingRequestsAsync()
    {
        Task<PendingFriendshipListResult> request = InvokeFriendServiceAsync(
            RequestPendingRequestsAsync,
            FriendResultMapper.CreateFailedRequestList);
        return request;
    }

    public Task<PlayerSearchResult> SearchPlayersAsync(string usernameFragment)
    {
        PlayerSearchRequest request = new PlayerSearchRequest
        {
            UsernameFragment = usernameFragment
        };
        Task<PlayerSearchResult> search = InvokeFriendServiceAsync(
            friendClient => RequestPlayerSearchAsync(friendClient, request),
            FriendResultMapper.CreateFailedPlayerSearch);
        return search;
    }

    public Task<FriendActionResult> SendFriendRequestAsync(string username)
    {
        FriendUsernameRequest request = new FriendUsernameRequest
        {
            Username = username
        };
        Task<FriendActionResult> command = SendCommandAsync(
            friendClient => friendClient.SendFriendRequestAsync(request));
        return command;
    }

    public Task<FriendActionResult> AcceptFriendRequestAsync(int requestId)
    {
        FriendshipDecisionRequest request = new FriendshipDecisionRequest
        {
            RequestId = requestId
        };
        Task<FriendActionResult> command = SendCommandAsync(
            friendClient => friendClient.AcceptFriendRequestAsync(request));
        return command;
    }

    public Task<FriendActionResult> RejectFriendRequestAsync(int requestId)
    {
        FriendshipDecisionRequest request = new FriendshipDecisionRequest
        {
            RequestId = requestId
        };
        Task<FriendActionResult> command = SendCommandAsync(
            friendClient => friendClient.RejectFriendRequestAsync(request));
        return command;
    }

    public Task<FriendActionResult> RemoveFriendAsync(string username)
    {
        FriendUsernameRequest request = new FriendUsernameRequest
        {
            Username = username
        };
        Task<FriendActionResult> command = SendCommandAsync(
            friendClient => friendClient.RemoveFriendAsync(request));
        return command;
    }

    public void Disconnect()
    {
        _friendClientSlot.Close();
    }

    private static async Task<FriendListResult> RequestFriendsAsync(FriendServiceClient friendClient)
    {
        IEnumerable<FriendResponse> responses = await friendClient.GetFriendsAsync().ConfigureAwait(false);
        List<FriendResult> friends = (responses ?? Enumerable.Empty<FriendResponse>())
            .Select(FriendResultMapper.ToFriendResult)
            .ToList();
        FriendListResult result = new FriendListResult(FriendActionResult.Success, friends);

        return result;
    }

    private static async Task<PendingFriendshipListResult> RequestPendingRequestsAsync(FriendServiceClient friendClient)
    {
        IEnumerable<PendingFriendshipResponse> responses =
            await friendClient.GetPendingRequestsAsync().ConfigureAwait(false);
        List<PendingFriendshipResult> requests = (responses ?? Enumerable.Empty<PendingFriendshipResponse>())
            .Select(FriendResultMapper.ToPendingFriendshipResult)
            .ToList();
        PendingFriendshipListResult result = new PendingFriendshipListResult(FriendActionResult.Success, requests);

        return result;
    }

    private static async Task<PlayerSearchResult> RequestPlayerSearchAsync(
        FriendServiceClient friendClient,
        PlayerSearchRequest request)
    {
        IEnumerable<PlayerSearchResponse> responses =
            await friendClient.SearchPlayersAsync(request).ConfigureAwait(false);
        List<PlayerSummary> players = (responses ?? Enumerable.Empty<PlayerSearchResponse>())
            .Select(response => new PlayerSummary(response.PlayerId, response.Username))
            .ToList();
        PlayerSearchResult result = new PlayerSearchResult(FriendActionResult.Success, players);

        return result;
    }

    private Task<FriendActionResult> SendCommandAsync(Func<FriendServiceClient, Task> command)
    {
        Task<FriendActionResult> commandTask = InvokeFriendServiceAsync(
            friendClient => RemoteCallGuard.CompleteCommandAsync(command(friendClient), FriendActionResult.Success),
            RemoteCallGuard.KeepStatus<FriendActionResult>);
        return commandTask;
    }

    private Task<TResult> InvokeFriendServiceAsync<TResult>(
        Func<FriendServiceClient, Task<TResult>> call,
        Func<FriendActionResult, TResult> createFailure)
    {
        string sessionToken = _gameServices.SessionToken;
        Task<TResult> invocation = _callGuard.ExecuteAsync(
            () => _friendClientSlot.CallAsync(sessionToken, call, () => OpenAndCallAsync(sessionToken, call)),
            _friendFailures,
            status => ReleaseClientWhenUnavailable(status, createFailure));
        return invocation;
    }

    private async Task<TResult> OpenAndCallAsync<TResult>(
        string sessionToken,
        Func<FriendServiceClient, Task<TResult>> call)
    {
        FriendServiceClient friendClient = _connectionFactory.CreateFriendClient(_callback);
        SessionTokenRequest request = new SessionTokenRequest
        {
            SessionToken = sessionToken
        };

        await _friendClientSlot.OpenAsync(friendClient, sessionToken, client => client.ConnectAsync(request))
            .ConfigureAwait(false);
        TResult result = await call(friendClient).ConfigureAwait(false);
        return result;
    }

    private TResult ReleaseClientWhenUnavailable<TResult>(
        FriendActionResult status,
        Func<FriendActionResult, TResult> createFailure)
    {
        if (status == FriendActionResult.ServerUnavailable)
        {
            _friendClientSlot.Close();
        }

        TResult failure = createFailure(status);
        return failure;
    }

    private void HandleFriendClientEnded(object sender, EventArgs eventArgs)
    {
        _logger.Warn("The friends channel ended. It will be opened again on the next friends request.");
    }
}
