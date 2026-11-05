using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteSocialServices : IRemoteSocialServices
{
    private static readonly ITraceLog _logger;

    private readonly ISocialConnectionFactory _connectionFactory;
    private readonly GameChannelCloser _channelCloser;
    private readonly IRemoteGameServices _gameServices;
    private readonly object _clientSyncRoot;
    private readonly SocialClientCallback _callback;
    private FriendServiceClient _friendClient;
    private string _connectedSessionToken;

    static RemoteSocialServices()
    {
        _logger = TraceLogManager.GetLogger(typeof(RemoteSocialServices));
    }

    public RemoteSocialServices(
        ISocialConnectionFactory connectionFactory,
        GameChannelCloser channelCloser,
        IRemoteGameServices gameServices)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _channelCloser = channelCloser ?? throw new ArgumentNullException(nameof(channelCloser));
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _clientSyncRoot = new object();
        _connectedSessionToken = string.Empty;
        _callback = CreateCallback();
    }

    public event EventHandler<FriendStatusChangedEventArgs> FriendStatusChanged;

    public event EventHandler<EventArgs> FriendsChanged;

    public async Task<FriendResultStatus> ConnectAsync()
    {
        FriendResultStatus status = await InvokeFriendServiceAsync(
            friendClient => Task.FromResult(FriendResultStatus.Success),
            KeepStatus).ConfigureAwait(false);
        return status;
    }

    public async Task<FriendListResult> GetFriendsAsync()
    {
        FriendListResult result = await InvokeFriendServiceAsync(
            RequestFriendsAsync,
            FriendResultMapper.CreateFailedFriendList).ConfigureAwait(false);
        return result;
    }

    public async Task<FriendRequestListResult> GetPendingRequestsAsync()
    {
        FriendRequestListResult result = await InvokeFriendServiceAsync(
            RequestPendingRequestsAsync,
            FriendResultMapper.CreateFailedRequestList).ConfigureAwait(false);
        return result;
    }

    public async Task<PlayerSearchResult> SearchPlayersAsync(string usernameFragment)
    {
        PlayerSearchRequest request = new PlayerSearchRequest
        {
            UsernameFragment = usernameFragment
        };
        PlayerSearchResult result = await InvokeFriendServiceAsync(
            friendClient => RequestPlayerSearchAsync(friendClient, request),
            FriendResultMapper.CreateFailedPlayerSearch).ConfigureAwait(false);
        return result;
    }

    public async Task<FriendResultStatus> SendFriendRequestAsync(string username)
    {
        FriendUsernameRequest request = new FriendUsernameRequest
        {
            Username = username
        };
        FriendResultStatus status = await InvokeFriendServiceAsync(
            friendClient => CompleteFriendCommandAsync(friendClient.SendFriendRequestAsync(request)),
            KeepStatus).ConfigureAwait(false);
        return status;
    }

    public async Task<FriendResultStatus> AcceptFriendRequestAsync(int requestId)
    {
        FriendshipDecisionRequest request = new FriendshipDecisionRequest
        {
            RequestId = requestId
        };
        FriendResultStatus status = await InvokeFriendServiceAsync(
            friendClient => CompleteFriendCommandAsync(friendClient.AcceptFriendRequestAsync(request)),
            KeepStatus).ConfigureAwait(false);
        return status;
    }

    public async Task<FriendResultStatus> RejectFriendRequestAsync(int requestId)
    {
        FriendshipDecisionRequest request = new FriendshipDecisionRequest
        {
            RequestId = requestId
        };
        FriendResultStatus status = await InvokeFriendServiceAsync(
            friendClient => CompleteFriendCommandAsync(friendClient.RejectFriendRequestAsync(request)),
            KeepStatus).ConfigureAwait(false);
        return status;
    }

    public async Task<FriendResultStatus> RemoveFriendAsync(string username)
    {
        FriendUsernameRequest request = new FriendUsernameRequest
        {
            Username = username
        };
        FriendResultStatus status = await InvokeFriendServiceAsync(
            friendClient => CompleteFriendCommandAsync(friendClient.RemoveFriendAsync(request)),
            KeepStatus).ConfigureAwait(false);
        return status;
    }

    public void Disconnect()
    {
        DiscardCurrentClient();
    }

    private static async Task<FriendListResult> RequestFriendsAsync(FriendServiceClient friendClient)
    {
        IEnumerable<FriendResponse> responses = await friendClient.GetFriendsAsync().ConfigureAwait(false);
        List<FriendResult> friends = (responses ?? Enumerable.Empty<FriendResponse>())
            .Select(FriendResultMapper.ToFriendResult)
            .ToList();
        FriendListResult result = new FriendListResult(FriendResultStatus.Success, friends);

        return result;
    }

    private static async Task<FriendRequestListResult> RequestPendingRequestsAsync(FriendServiceClient friendClient)
    {
        IEnumerable<PendingFriendRequestResponse> responses =
            await friendClient.GetPendingRequestsAsync().ConfigureAwait(false);
        List<FriendRequestResult> requests = (responses ?? Enumerable.Empty<PendingFriendRequestResponse>())
            .Select(FriendResultMapper.ToFriendRequestResult)
            .ToList();
        FriendRequestListResult result = new FriendRequestListResult(FriendResultStatus.Success, requests);

        return result;
    }

    private static async Task<PlayerSearchResult> RequestPlayerSearchAsync(
        FriendServiceClient friendClient,
        PlayerSearchRequest request)
    {
        IEnumerable<PlayerSearchResponse> responses =
            await friendClient.SearchPlayersAsync(request).ConfigureAwait(false);
        List<FoundPlayerResult> players = (responses ?? Enumerable.Empty<PlayerSearchResponse>())
            .Select(response => new FoundPlayerResult(response.PlayerId, response.Username))
            .ToList();
        PlayerSearchResult result = new PlayerSearchResult(FriendResultStatus.Success, players);

        return result;
    }

    private static async Task<FriendResultStatus> CompleteFriendCommandAsync(Task command)
    {
        await command.ConfigureAwait(false);
        FriendResultStatus status = FriendResultStatus.Success;
        return status;
    }

    private static FriendResultStatus KeepStatus(FriendResultStatus status)
    {
        return status;
    }

    private async Task<TResult> InvokeFriendServiceAsync<TResult>(
        Func<FriendServiceClient, Task<TResult>> call,
        Func<FriendResultStatus, TResult> createFailure)
    {
        TResult result;

        try
        {
            FriendServiceClient friendClient = await GetConnectedClientAsync().ConfigureAwait(false);
            result = await call(friendClient).ConfigureAwait(false);
        }
        catch (FaultException<GameServiceFault> exception)
        {
            _logger.WarnFormat("The server rejected a friends request. FaultCode: {0}.", exception.Detail.Code);
            result = createFailure(FriendResultMapper.MapFault(exception.Detail.Code));
        }
        catch (FaultException exception)
        {
            _logger.Error("The server reported an undeclared fault while processing a friends request.", exception);
            result = createFailure(FriendResultStatus.ServerError);
        }
        catch (TimeoutException exception)
        {
            _logger.Warn("A friends request did not finish before the configured timeout.", exception);
            DiscardCurrentClient();
            result = createFailure(FriendResultStatus.ServerUnavailable);
        }
        catch (CommunicationException exception)
        {
            _logger.Warn("A friends request failed because of a communication error.", exception);
            DiscardCurrentClient();
            result = createFailure(FriendResultStatus.ServerUnavailable);
        }
        catch (ObjectDisposedException exception)
        {
            _logger.Warn("A friends request used a channel that was already closed.", exception);
            DiscardCurrentClient();
            result = createFailure(FriendResultStatus.ServerUnavailable);
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error("The friends endpoint is missing or invalid in App.config.", exception);
            result = createFailure(FriendResultStatus.ServerError);
        }

        return result;
    }

    private async Task<FriendServiceClient> GetConnectedClientAsync()
    {
        string sessionToken = _gameServices.SessionToken;
        FriendServiceClient friendClient;
        bool isReusable;

        lock (_clientSyncRoot)
        {
            friendClient = _friendClient;
            isReusable = friendClient?.State == CommunicationState.Opened
                && string.Equals(_connectedSessionToken, sessionToken, StringComparison.Ordinal);
        }

        if (!isReusable)
        {
            friendClient = await OpenFriendClientAsync(sessionToken).ConfigureAwait(false);
        }

        return friendClient;
    }

    private async Task<FriendServiceClient> OpenFriendClientAsync(string sessionToken)
    {
        FriendServiceClient friendClient = _connectionFactory.CreateFriendClient(_callback);
        bool isConnected = false;

        try
        {
            ConnectRequest request = new ConnectRequest
            {
                SessionToken = sessionToken
            };

            await friendClient.ConnectFriendsAsync(request).ConfigureAwait(false);
            ReplaceCurrentClient(friendClient, sessionToken);
            isConnected = true;
        }
        finally
        {
            if (!isConnected)
            {
                _channelCloser.Close(friendClient);
            }
        }

        return friendClient;
    }

    private void ReplaceCurrentClient(FriendServiceClient friendClient, string sessionToken)
    {
        ICommunicationObject channel = friendClient;
        FriendServiceClient previousClient;

        channel.Faulted += (sender, e) => DiscardEndedClient(friendClient);
        channel.Closed += (sender, e) => DiscardEndedClient(friendClient);

        lock (_clientSyncRoot)
        {
            previousClient = _friendClient;
            _friendClient = friendClient;
            _connectedSessionToken = sessionToken;
        }

        if (previousClient != null)
        {
            _channelCloser.Close(previousClient);
        }
    }

    private void DiscardCurrentClient()
    {
        FriendServiceClient discardedClient;

        lock (_clientSyncRoot)
        {
            discardedClient = _friendClient;
            _friendClient = null;
            _connectedSessionToken = string.Empty;
        }

        if (discardedClient != null)
        {
            _channelCloser.Close(discardedClient);
        }
    }

    private void DiscardEndedClient(FriendServiceClient endedClient)
    {
        bool isCurrentClient;

        lock (_clientSyncRoot)
        {
            isCurrentClient = ReferenceEquals(endedClient, _friendClient);

            if (isCurrentClient)
            {
                _friendClient = null;
                _connectedSessionToken = string.Empty;
            }
        }

        if (isCurrentClient)
        {
            _logger.Warn("The friends channel ended. It will be opened again on the next friends request.");
            _channelCloser.Close(endedClient);
        }
    }

    private SocialClientCallback CreateCallback()
    {
        SocialClientCallback callback = new SocialClientCallback();
        callback.FriendStatusChanged += HandleFriendStatusChanged;
        callback.FriendsChanged += HandleFriendsChanged;
        return callback;
    }

    private void HandleFriendStatusChanged(object sender, FriendStatusChangedEventArgs e)
    {
        OnFriendStatusChanged(e);
    }

    private void HandleFriendsChanged(object sender, EventArgs e)
    {
        OnFriendsChanged(e);
    }

    private void OnFriendStatusChanged(FriendStatusChangedEventArgs eventArgs)
    {
        FriendStatusChanged?.Invoke(this, eventArgs);
    }

    private void OnFriendsChanged(EventArgs eventArgs)
    {
        FriendsChanged?.Invoke(this, eventArgs);
    }
}
